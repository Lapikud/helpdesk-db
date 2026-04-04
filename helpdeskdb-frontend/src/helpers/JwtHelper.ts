import { jwtDecode } from "jwt-decode";

interface DecodedToken {
	[claim: string]: unknown;
}

export function getRolesFromToken(token: string): string[] {
	const decoded: DecodedToken = jwtDecode<DecodedToken>(token);
	const rolesClaim: string = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

	if (rolesClaim in decoded) {
		const roles = decoded[rolesClaim];

		if (Array.isArray(roles)) {
			// Verify all array elements are strings
			if (roles.every(item => typeof item === "string")) {
				for (let i = 0; i < roles.length; i++) {
					const element = roles[i];
					console.log("role: " + element);
				}
				return roles;
			}
		} else if (typeof roles === "string") {
			return [roles];
		}
	}
	return [];
}

export function getUsernameFromToken(token: string): string {
	const decoded: DecodedToken = jwtDecode<DecodedToken>(token);
	const nameClaim: string = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

	if (nameClaim in decoded) {
		const name = decoded[nameClaim];
		if (typeof name === "string") {
			return name;
		}
	}
	return "";
}

export function getUserIdFromToken(token: string): string {
	const decoded: DecodedToken = jwtDecode<DecodedToken>(token);
	const nameIdentifierClaim: string = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

	if (nameIdentifierClaim in decoded) {
		const name = decoded[nameIdentifierClaim];
		if (typeof name === "string") {
			return name;
		}
	}
	return "";
}