import { apiDelete, apiGet, apiPost, apiPut, type ApiFetch } from '$lib/api/apiClient';
import type { BasicResultDto, ChangeUserNameRequest, UpdateProfileRequest } from '$lib/types/auth';
import type { PersonalDataDto, PublicUserDto, UserDto } from '$lib/types/user';

export function getUsers(fetchFn: ApiFetch) {
	return apiGet<UserDto[]>(fetchFn, '/api/user/all');
}

export function getPublicUsers(fetchFn: ApiFetch) {
	return apiGet<PublicUserDto[]>(fetchFn, '/api/user/public-lite');
}

export function getUserRoles(fetchFn: ApiFetch, id: string) {
	return apiGet<string[]>(fetchFn, `/api/user/${id}/roles`);
}

export function addUserRole(fetchFn: ApiFetch, id: string, roleName: string) {
	return apiPost<void>(fetchFn, `/api/user/${id}/add-role/${encodeURIComponent(roleName)}`, undefined, {
		emptyResponse: true
	});
}

export function removeUserRole(fetchFn: ApiFetch, id: string, roleName: string) {
	return apiDelete<void>(fetchFn, `/api/user/${id}/remove-role/${encodeURIComponent(roleName)}`, undefined, {
		emptyResponse: true
	});
}

export function changeMyUsername(fetchFn: ApiFetch, request: ChangeUserNameRequest) {
	return apiPut<BasicResultDto>(fetchFn, '/api/user/me/username', request);
}

export function changeUserName(fetchFn: ApiFetch, id: string, request: ChangeUserNameRequest) {
	return apiPut<BasicResultDto>(fetchFn, `/api/user/${id}/username`, request);
}

export function sendResetLink(fetchFn: ApiFetch, email: string) {
	return apiPost<BasicResultDto>(fetchFn, '/api/auth/send-reset-link', { email });
}

export function updateMyProfile(fetchFn: ApiFetch, request: UpdateProfileRequest) {
	return apiPut<BasicResultDto>(fetchFn, '/api/users/me/profile', request);
}

export function getMyPersonalData(fetchFn: ApiFetch) {
	return apiGet<PersonalDataDto>(fetchFn, '/api/users/me/personal-data');
}

export function deleteUser(fetchFn: ApiFetch, id: string) {
	return apiDelete<void>(fetchFn, `/api/user/delete/${id}`, undefined, { emptyResponse: true });
}
