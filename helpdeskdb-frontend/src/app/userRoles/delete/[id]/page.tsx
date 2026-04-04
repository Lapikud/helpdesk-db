"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { UserService } from "@/services/UserService";
import { UserRoleService } from "@/services/UserRoleService";
import { RoleService } from "@/services/RoleService";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IUserRoleWithUsernameAndRoleName } from "@/types/domain/DomainTypes";

export default function UserRoleDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tUserRole } = useTranslation("appuserRole");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<IUserRoleWithUsernameAndRoleName>();
	const userRoleService: UserRoleService = useMemo(
		() => new UserRoleService(),
		[]
	);
	const userService: UserService = useMemo(() => new UserService(), []);
	const roleService: RoleService = useMemo(() => new RoleService(), []);
	if (setAccountInfo) {
		userRoleService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		roleService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		} else if (!isAdmin) {
			router.push("/userRoles");
		}
		const fetchData = async () => {
			try {
				const result = await userRoleService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const userRole = result.data!;

				const userResult = (await userService.getAsync(userRole.userId));
				let username;
				if (userResult.errors) {
					console.log(userResult.errors);
					return;
				} else {
					username = userResult.data?.userName!;
				}

				const roleResult = (await roleService.getAsync(userRole.roleId));
				let rolename;
				if (roleResult.errors) {
					console.log(roleResult.errors);
					return;
				} else {
					rolename = roleResult.data?.name!;
				}

				setData({...userRole, username, rolename});
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, accountInfo, router, id, isAdmin, userRoleService]);

	const deleteConfirmed = async () => {
		try {
			const result = await userRoleService.deleteAsync(id);

			console.log("delete result", result);

			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/userRoles");
			}
		} catch (error) {
			console.log("error: ", (error as Error).message);
			setErrorMessage((error as Error).message);
		}
	};

	if (!hydrated || !data) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("DeleteTitle")}
			</h1>

			<h3 className="text-lg text-gray-700 mb-4">
				{tCommon("DeleteConfirmQuestion")}
			</h3>
			<div className="bg-white p-6 rounded-lg shadow-md max-w-xl mx-auto space-y-4">
				<h4 className="text-xl font-medium text-gray-800">
					UserRole{/* {tUserRole("AppUserRoleName")} */}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tUserRole("AppUser")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.username}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tUserRole("AppRole")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.rolename}</dd>
					</div>
				</dl>
				<div className="mt-6 flex items-center space-x-4 justify-center">
					<button
						onClick={() => deleteConfirmed()}
						type="button"
						title="Delete"
						className="bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-4 rounded transition"
					>
						{tCommon("DeleteButton")}
					</button>

					<span className="text-gray-400">|</span>

					<Link
						href="/userRoles"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
