import en from '../public/i18n/en.json';
import fr from '../public/i18n/fr.json';

// The keys manager (nx run web:i18n-find) finds keys used in the code but missing from the
// files. It can't tell when a key is only in one language: the tag keys come from the API, so
// the files always have keys the code doesn't name.

type Translation = { [key: string]: string | Translation };

function flatten(translation: Translation, prefix = ''): Map<string, string> {
  const entries = new Map<string, string>();
  for (const [key, value] of Object.entries(translation)) {
    if (typeof value === 'string') {
      entries.set(prefix + key, value);
    } else {
      flatten(value, `${prefix}${key}.`).forEach((v, k) => entries.set(k, v));
    }
  }
  return entries;
}

const params = (value: string) =>
  [...value.matchAll(/{{\s*(\w+)\s*}}/g)].map((match) => match[1]).sort();

describe('translation files', () => {
  const english = flatten(en);
  const french = flatten(fr);

  it('have the same keys in English and French', () => {
    expect([...french.keys()].sort()).toEqual([...english.keys()].sort());
  });

  it('use the same params in both languages', () => {
    const different = [...english]
      .filter(([key]) => french.has(key))
      .filter(
        ([key, value]) =>
          params(value).join() !== params(french.get(key) ?? '').join(),
      )
      .map(([key]) => key);
    expect(different).toEqual([]);
  });

  it('have no empty translation', () => {
    const empty = [...english, ...french]
      .filter(([, value]) => !value.trim())
      .map(([key]) => key);
    expect(empty).toEqual([]);
  });
});
