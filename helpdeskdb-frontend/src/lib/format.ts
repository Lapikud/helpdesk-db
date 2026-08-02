/** Shared date formatting — locale-default, matching the existing UI. */

export const formatDate = (value: string | Date): string =>
	new Date(value).toLocaleDateString();

export const formatDateTime = (value: string | Date): string =>
	new Date(value).toLocaleString();
