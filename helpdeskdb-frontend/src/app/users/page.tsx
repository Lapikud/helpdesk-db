"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { UserService } from "@/services/UserService";
import { RoleService } from "@/services/RoleService";
import { UserRoleService } from "@/services/UserRoleService";
import { useRouter } from "next/navigation";

import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { IUserWithRoles } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";

export default function Users() {
	const { t: tUser } = useTranslation("appuser");
	const { t: tRole } = useTranslation("approle");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const userService: UserService = useMemo(() => new UserService(), []);
	const roleService: RoleService = useMemo(() => new RoleService(), []);
	const userRoleService: UserRoleService = useMemo(
		() => new UserRoleService(),
		[],
	);
	if (setAccountInfo) {
		userService.injectSetAccountInfo(setAccountInfo);
		roleService.injectSetAccountInfo(setAccountInfo);
		userRoleService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IUserWithRoles[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const [usersResult, rolesResult, userRolesResult] = await Promise.all([
			userService.getAllAsync(),
			roleService.getAllAsync(),
			userRoleService.getAllAsync(),
		]);

		if (
			usersResult.errors ||
			!usersResult.data ||
			rolesResult.errors ||
			!rolesResult.data ||
			userRolesResult.errors ||
			!userRolesResult.data
		) {
			return;
		}

		const roleNameById = new Map(
			rolesResult.data.map((r) => [r.id, r.name]),
		);

		const withRoles: IUserWithRoles[] = usersResult.data.map((user) => ({
			...user,
			roles: userRolesResult
				.data!.filter((ur) => ur.userId === user.id)
				.map((ur) => roleNameById.get(ur.roleId))
				.filter(Boolean) as string[],
		}));

		setData(withRoles);
	}, [userService, roleService, userRoleService]);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [hydrated, router, isAdmin, fetchData]);

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = [tUser("AppUserName"), tRole("AppRoles")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [item.username, item.roles.join(", ") || "-"],
	}));

	return (
		<ListPageWrapper title={tUser("AppUsers")}>
			<DataTable columns={columns} rows={rows} />
		</ListPageWrapper>
	);
}
