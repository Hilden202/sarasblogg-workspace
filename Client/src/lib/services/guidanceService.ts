import { apiPost, type ApiFetch } from '$lib/api/apiClient';
import type { GuidanceRequest, GuidanceResponse } from '$lib/types/guidance';

export function requestGuidance(fetchFn: ApiFetch, request: GuidanceRequest) {
	return apiPost<GuidanceResponse>(fetchFn, '/api/guidance', request);
}
