"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { UserService } from "@/services/UserService";
import { UserRoleService } from "@/services/UserRoleService";
import { RoleService } from "@/services/RoleService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import Spinner from "@/components/LoadingSpinner";
import { IUserRoleWithUsernameAndRoleName } from "@/types/domain/DomainTypes";

export default function UserRoles() {
	const { t: tUserRole } = useTranslation("appUserRole");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const userRoleService: UserRoleService = useMemo(() => new UserRoleService(), []);
	const userService: UserService = useMemo(() => new UserService(), []);
	const roleService: RoleService = useMemo(() => new RoleService(), []);
	if (setAccountInfo) {
		userRoleService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		roleService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IUserRoleWithUsernameAndRoleName[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

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

		const fetchData = async () => {
			try {
				const result = await userRoleService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const userRolesWithNames = await Promise.all(
                    result.data!.map(async (userRole) => {
                        const user = await userService.getAsync(userRole.userId);
                        const role = await roleService.getAsync(userRole.roleId);
                        const username = user.data?.userName ?? userRole.userId;
                        const rolename = role.data?.name ?? userRole.roleId;
                        return { ...userRole, username, rolename };
                    })
                );
                setData(userRolesWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, userRoleService, userService, roleService, isAdmin]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">UserRoles</h1>

			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/userRoles/create"
						className="inline-block bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
					>
						{tCommon("CreateNewLink")}
					</Link>
				</p>
			)}

			<div className="w-full max-w-7xl overflow-x-auto shadow rounded-lg">
				<table className="w-full table-auto bg-white border border-gray-200 text-left">
					<thead className="bg-gray-100">
						<tr>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tUserRole("AppUser")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tUserRole("AppRole")}
							</th>
							{isAdmin && (
								<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
									{tCommon("Actions")}
								</th>
							)}
						</tr>
					</thead>
					<tbody>
						{data.map((item) => (
							<tr key={item.id} className="hover:bg-gray-50">
								<td className="px-6 py-4 border-b">
									{item.username}
								</td>
								<td className="px-6 py-4 border-b">
									{item.rolename}
								</td>
								<td className="px-6 py-4 border-b text-blue-600 space-x-2">
									<Link
										href={`/userRoles/edit/${item.id}`}
										className="hover:underline"
									>
										{tCommon("EditLink")}
									</Link>
									<span className="text-gray-400">|</span>
									<Link
										href={`/userRoles/details/${item.id}`}
										className="hover:underline"
									>
										{tCommon("DetailsLink")}
									</Link>
									<span className="text-gray-400">|</span>
									<Link
										href={`/userRoles/delete/${item.id}`}
										className="hover:underline"
									>
										{tCommon("DeleteLink")}
									</Link>
								</td>
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</>
	);
}
