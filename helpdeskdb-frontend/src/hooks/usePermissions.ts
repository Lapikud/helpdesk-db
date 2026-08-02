"use client";

import { useContext } from "react";
import { AccountContext, IAccountInfo } from "@/context/AccountContext";
import { ROLES } from "@/lib/roles";

export interface Permissions {
	/** Raw context value — undefined while the /me call is still hydrating. */
	accountInfo: IAccountInfo | undefined;
	userId: string | undefined;
	userName: string | undefined;
	isAdmin: boolean;
	isHelpdeskDbAdmin: boolean;
	/**
	 * Gates create/edit/delete UI, admin-only pages, and the Header admin
	 * dropdown; matches the backend's
	 * [Authorize(Roles = "admins,helpdesk_db_admins")] on write endpoints.
	 */
	canManage: boolean;
	/**
	 * Gates the Actions columns on the reservations page and the overview
	 * AssetList (managers plus members/pixels).
	 */
	canSeeReservationActions: boolean;
}

export function usePermissions(): Permissions {
	const { accountInfo } = useContext(AccountContext);
	const roles = accountInfo?.roles ?? [];

	const isAdmin = roles.includes(ROLES.admins);
	const isHelpdeskDbAdmin = roles.includes(ROLES.helpdeskDbAdmins);
	const canManage = isAdmin || isHelpdeskDbAdmin;

	return {
		accountInfo,
		userId: accountInfo?.id,
		userName: accountInfo?.name,
		isAdmin,
		isHelpdeskDbAdmin,
		canManage,
		canSeeReservationActions:
			canManage ||
			roles.includes(ROLES.members) ||
			roles.includes(ROLES.pixels),
	};
}
