"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { UserRoleService } from "@/services/UserRoleService";
import { UserService } from "@/services/UserService";
import { RoleService } from "@/services/RoleService";
import { IUser, IRole, IUserRole } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function UserRoleEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tUserRole } = useTranslation("appUserRole");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	const [usersData, setUsersData] = useState<IUser[]>([]);
	const [rolesData, setRolesData] = useState<IRole[]>([]);

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

	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
	} = useForm<IUserRole>();

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		const fetchData = async () => {
			try {
				setIsLoading(true);

				// get this userRole
				const thisUserRole = await userRoleService.getAsync(id);

				if (thisUserRole.errors || !thisUserRole.data) {
					setErrorMessage(
						thisUserRole.errors?.join(", ") ||
							"Failed to load user role"
					);
					return;
				}

				// get all users
				const usersResult = await userService.getAllAsync();
				if (usersResult.errors) {
					console.log(usersResult.errors);
					return;
				}

				// set usersData to this current user
				setUsersData(
					usersResult.data!.filter(
						(u) => u.id === thisUserRole.data!.userId
					)
				);

				// get all roles
				const rolesResult = await roleService.getAllAsync();
				if (rolesResult.errors) {
					console.log(rolesResult.errors);
					return;
				}

				// get all userRoles for this user
				const allUserRolesResult = await userRoleService.getAllAsync();
				if (allUserRolesResult.errors) {
					console.log(allUserRolesResult.errors);
					return;
				}

				const userRolesForThisUser = allUserRolesResult.data!.filter(
					(ur) => ur.userId === thisUserRole.data!.userId
				);

				// exclude roles the user already has, except the current one
				const otherRoleIds = userRolesForThisUser
					.filter((ur) => ur.id !== thisUserRole.data!.id)
					.map((ur) => ur.roleId);


				setRolesData(
					rolesResult.data!.filter(
						(r) =>
							!otherRoleIds.includes(r.id) ||
							r.id === thisUserRole.data!.roleId
					)
				);

				reset({
					roleId: thisUserRole.data.roleId,
					userId: thisUserRole.data.userId,
				});
			} catch (error) {
				console.error("error fetching data: ", error);
			} finally {
				setIsLoading(false);
			}
		};
		fetchData();
	}, [
		hydrated,
		accountInfo,
		router,
		id,
		reset,
		isAdmin,
		userService,
		roleService,
		userRoleService
	]);

	const onSubmit: SubmitHandler<IUserRole> = async (data: IUserRole) => {
		setErrorMessage("Loading...");
		try {
			const result = await userRoleService.updateAsync({
				id: id,
				roleId: data.roleId,
				userId: data.userId,
			});
			console.log("edit result", result);

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

	if (!hydrated || isLoading) {
		return <Spinner className="h-64" />;
	}

	if (!accountInfo?.id || !isAdmin) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("EditTitle")}
			</h1>
			<h4 className="text-xl text-gray-700 mb-4">
				UserRole{/* {tUserRole("AppUserRoleName")} */}
			</h4>
			<hr className="mb-6 border-gray-300" />

			<div className="max-w-md mx-auto">
				<form
					onSubmit={handleSubmit(onSubmit)}
					className="bg-white p-6 rounded-lg shadow-md space-y-5"
				>
					{errorMessage.length > 0 && (
						<p className="text-red-600">{errorMessage}</p>
					)}
					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="AppUser"
						>
							{tUserRole("AppUser")}
						</label>
						<select
							id="userId"
							{...register("userId", {
								required: tValidation("Required", {
									field: tUserRole("AppUser"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tUserRole("AppUser")}
							</option>
							{usersData.map((user) => (
								<option key={user.id} value={user.id}>
									{user.username}
								</option>
							))}
						</select>
						{errors.userId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.userId.message}
							</p>
						)}
					</div>
					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="AppRole"
						>
							{tUserRole("AppRole")}
						</label>
						<select
							id="roleId"
							{...register("roleId", {
								required: tValidation("Required", {
									field: tUserRole("AppRole"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tUserRole("AppRole")}
							</option>
							{rolesData.map((role) => (
								<option key={role.id} value={role.id}>
									{role.name}
								</option>
							))}
						</select>
						{errors.roleId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.roleId.message}
							</p>
						)}
					</div>

					<div>
						<button
							type="submit"
							className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition"
						>
							{tCommon("CreateButton")}
						</button>
					</div>
				</form>
			</div>

			<div className="mt-6 text-center">
				<Link
					href="/userRoles"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
