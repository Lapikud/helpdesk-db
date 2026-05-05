"use client";

import Link from "next/link";
import { useContext, useEffect, useState } from "react";
import { UserRoleService } from "@/services/UserRoleService";
import { UserService } from "@/services/UserService";
import { RoleService } from "@/services/RoleService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IUser, IRole, IUserRoleAdd } from "@/types/domain/DomainTypes";

export default function UserRoleCreate() {
	const { t: tUserRole } = useTranslation("appUserRole");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [usersData, setUsersData] = useState<IUser[]>([]);
	const [rolesData, setRolesData] = useState<IRole[]>([]);
	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		const fetchData = async () => {
			try {
				const userService: UserService = new UserService();
				const roleService: RoleService = new RoleService();
				if (setAccountInfo) {
					userService.injectSetAccountInfo(setAccountInfo);
					roleService.injectSetAccountInfo(setAccountInfo);
				}

				const usersResult = await userService.getAllAsync();
				if (usersResult.errors) {
					console.log(usersResult.errors);
					return;
				}
				setUsersData(usersResult.data!);

				const rolesResult = await roleService.getAllAsync();
				if (rolesResult.errors) {
					console.log(rolesResult.errors);
					return;
				}
				setRolesData(rolesResult.data!);
			} catch (error) {
				console.error("error fetching data: ", error)
			}
		};
		fetchData();
	}, [hydrated, setAccountInfo]);

	const [errorMessage, setErrorMessage] = useState("");

	const userRoleService: UserRoleService = new UserRoleService();

	if (setAccountInfo) {
		userRoleService.injectSetAccountInfo(setAccountInfo);
	}


	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<IUserRoleAdd>({});

	const onSubmit: SubmitHandler<IUserRoleAdd> = async (data: IUserRoleAdd) => {
		console.log(data);
		setErrorMessage("Loading...");

		try {
			const result = await userRoleService.addAsync(data);

			if (result.errors) {
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

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	if (!accountInfo?.id || !isAdmin) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("CreateTitle")}
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
