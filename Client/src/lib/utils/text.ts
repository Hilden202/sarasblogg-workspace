export function stripHtml(value?: string | null) {
	if (!value) return '';
	return value
		.replace(/<[^>]*>/g, ' ')
		.replace(/\s+/g, ' ')
		.trim();
}

export function truncate(value?: string | null, length = 180) {
	const text = stripHtml(value);
	if (text.length <= length) return text;
	return `${text.slice(0, length).trimEnd()}...`;
}

export function initials(value?: string | null) {
	const source = value?.trim() || 'SarasBlogg';
	return source
		.split(/\s+/)
		.slice(0, 2)
		.map((part) => part[0]?.toUpperCase())
		.join('');
}
