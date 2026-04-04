"use client";

import Link from "next/link";
import { useContext, useEffect, useState } from "react";
import { RoleService } from "@/services/RoleService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function RoleCreate() {
	const { t: tRole } = useTranslation("approle");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
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
	}, [hydrated, accountInfo, router, isAdmin]);

	const [errorMessage, setErrorMessage] = useState("");

	const roleService: RoleService = new RoleService();

	if (setAccountInfo) {
		roleService.injectSetAccountInfo(setAccountInfo);
	}

	type Inputs = {
		name: string;
	};

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<Inputs>({});

	const onSubmit: SubmitHandler<Inputs> = async (data: Inputs) => {
		console.log(data);
		setErrorMessage("Loading...");

		try {
			const result = await roleService.addAsync({
				name: data.name,
			});

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/roles");
			}
		} catch (error) {
			console.log("error: ", (error as Error).message);
			setErrorMessage((error as Error).message);
		}
	};

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	if (!accountInfo?.jwt || !isAdmin) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("CreateTitle")}
			</h1>
			<h4 className="text-xl text-gray-700 mb-4">
				{tRole("AppRoleName")}
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
							htmlFor="AppRoleName"
						>
							{tRole("AppRoleName")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="name"
							placeholder={tRole("AppRoleNamePrompt")}
							{...register("name", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tRole("AppRoleName"),
									}),
								},
								minLength: {
									value: 2,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tRole("AppRoleName"),
											min: 2,
										}
									),
								},
								maxLength: {
									value: 128,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tRole("AppRoleName"),
											max: 128,
										}
									),
								},
							})}
						/>
						{errors.name && (
							<span className="text-red-600 text-sm">
								{errors.name.message}
							</span>
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
				<Link href="/roles" className="text-blue-600 hover:underline">
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
