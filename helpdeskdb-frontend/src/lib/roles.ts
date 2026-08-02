/**
 * Role names as issued by FreeIPA and synced into the local AppUserRole
 * table. The only sanctioned way to check roles in the UI is through
 * `usePermissions()` — never compare against bare string literals.
 */
export const ROLES = {
	admins: "admins",
	helpdeskDbAdmins: "helpdesk_db_admins",
	members: "members",
	pixels: "pixels",
} as const;

export type Role = (typeof ROLES)[keyof typeof ROLES];
