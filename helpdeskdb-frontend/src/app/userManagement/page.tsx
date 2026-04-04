"use client";

import Spinner from "@/components/LoadingSpinner";
import { AccountContext } from "@/context/AccountContext";
import { RoleService } from "@/services/RoleService";
import { UserManagementService } from "@/services/UserManagementService";
import { UserRoleService } from "@/services/UserRoleService";
import { UserService } from "@/services/UserService";
import { IRole, IUser } from "@/types/domain/DomainTypes";
import { useRouter } from "next/navigation";
import { useContext, useEffect, useMemo, useState } from "react";

export default function UserManagement() {
	const [users, setUsers] = useState<(IUser & { roles: string[] })[]>([]);
	const [roles, setRoles] = useState<IRole[]>([]);
	const [loading, setLoading] = useState(true);

	const { accountInfo, setAccountInfo } = useContext(AccountContext);

	const userRoleService: UserRoleService = useMemo(
		() => new UserRoleService(),
		[]
	);
	const userService: UserService = useMemo(() => new UserService(), []);
	const roleService: RoleService = useMemo(() => new RoleService(), []);
	const userManagementService: UserManagementService = useMemo(
		() => new UserManagementService(),
		[]
	);

	if (setAccountInfo) {
		userRoleService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		roleService.injectSetAccountInfo(setAccountInfo);
		userManagementService.injectSetAccountInfo(setAccountInfo);
	}

	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	const fetchData = async () => {
		setLoading(true);
		const usersResult = await userService.getAllAsync();
		const rolesResult = await roleService.getAllAsync();
		const userRolesResult = await userRoleService.getAllAsync();

		const users = usersResult.data ?? [];
		const roles = rolesResult.data ?? [];
		const userRoles = userRolesResult.data ?? [];

		// Combine users with their roles
		const usersWithRoles = users.map((user) => {
			const userRoleEntries = userRoles.filter(
				(ur) => ur.userId === user.id
			);
			const roleNames = userRoleEntries
				.map((ur) => roles.find((r) => r.id === ur.roleId)?.name)
				.filter(Boolean) as string[];
			return { ...user, roles: roleNames };
		});

		setUsers(usersWithRoles);
		setRoles(roles);
		setLoading(false);
	};

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		}

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [
		hydrated,
		loading,
		accountInfo,
		router,
		userService,
		roleService,
		userRoleService,
	]);

	// Add role to user
	const handleAddRole = async (userId: string, roleId: string) => {
		await userManagementService.addRole(userId, roleId);
		await fetchData();
	};

	// Remove role from user
	const handleRemoveRole = async (userId: string, roleId: string) => {
		await userManagementService.removeRole(userId, roleId);
		await fetchData();
	};

	// Reset password link
	const handleResetPassword = async (userId: string) => {
		const res = await userManagementService.getPasswordResetLink(userId);
		const data = res.data!;
		alert(
			`Password reset link for user: ${data.email}\n${data.passwordResetLink}`
		);
	};

	if (!hydrated || loading) {
		return <Spinner className="h-64" />;
	}

	return (
		<div>
			<h1 className="text-2xl font-bold mb-4">Users</h1>
			<table className="min-w-full border">
				<thead>
					<tr>
						<th className="border px-2 py-1">Username</th>
						<th className="border px-2 py-1">Roles</th>
						<th className="border px-2 py-1">Password reset</th>
					</tr>
				</thead>
				<tbody>
					{users.map((user) => (
						<tr key={user.id}>
							<td className="border px-2 py-1">
								{user.userName}
							</td>
							<td className="border px-2 py-1">
								<ul>
									{roles.map((role) => (
										<li key={role.id}>
											{role.name}{" "}
											{user.roles.includes(role.name) ? (
												<button
													className="text-red-600 underline"
													onClick={() =>
														handleRemoveRole(
															user.id,
															role.id
														)
													}
												>
													Remove
												</button>
											) : (
												<button
													className="text-blue-600 underline"
													onClick={() =>
														handleAddRole(
															user.id,
															role.id
														)
													}
												>
													Add
												</button>
											)}
										</li>
									))}
								</ul>
							</td>
							<td className="border px-2 py-1">
								<button
									className="text-green-600 underline"
									onClick={() => handleResetPassword(user.id)}
								>
									Reset PWD
								</button>
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</div>
	);
}
