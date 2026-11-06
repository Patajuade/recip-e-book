import { HttpErrorResponse } from '@angular/common/http';
import {
  apply,
  email,
  FieldTree,
  maxLength,
  minLength,
  required,
  schema,
  ValidationError,
  WithOptionalFieldTree,
} from '@angular/forms/signals';

export interface Credentials {
  email: string;
  password: string;
}

export const emptyCredentials = (): Credentials => ({
  email: '',
  password: '',
});

// Mirrors the API rules (LoginInput, RegisterInput). The minimum password length only applies
// when creating an account: an existing password is for the server to judge.
export const signInSchema = schema<Credentials>((credentials) => {
  required(credentials.email);
  email(credentials.email);
  maxLength(credentials.email, 256);
  required(credentials.password);
  maxLength(credentials.password, 128);
});

export const signUpSchema = schema<Credentials>((credentials) => {
  apply(credentials, signInSchema);
  minLength(credentials.password, 15);
});

export const isStatus = (error: unknown, status: number): boolean =>
  error instanceof HttpErrorResponse && error.status === status;

/** The API's 400 errors, placed under their field; empty for any other failure. */
export function toCredentialErrors(
  error: unknown,
  credentialsForm: FieldTree<Credentials>,
): WithOptionalFieldTree<ValidationError>[] {
  const errors: Record<string, string[]> | undefined = isStatus(error, 400)
    ? (error as HttpErrorResponse).error?.errors
    : undefined;
  const fields: Record<string, FieldTree<unknown>> = {
    Email: credentialsForm.email,
    Password: credentialsForm.password,
  };
  return Object.entries(errors ?? {})
    .filter(([path]) => path in fields)
    .flatMap(([path, messages]) =>
      messages.map((message) => ({
        kind: 'server',
        message,
        fieldTree: fields[path],
      })),
    );
}
