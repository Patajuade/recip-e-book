import { httpResource } from '@angular/common/http';
import { TagSummary } from '@recip-e-book/web-recipes-models';

export function tagCatalogResource() {
  return httpResource<TagSummary[]>(() => '/api/tags');
}
