"use client";

import { AccountContext } from "@/context/AccountContext";
import { AccountService } from "@/services/AccountService";
import { useRouter } from "next/navigation";
import { useContext, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import { getRolesFromToken, getUserIdFromToken, getUsernameFromToken } from "@/helpers/JwtHelper";
import { useTranslation } from "react-i18next";

export default function Login() {
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { t: tIdentityerrors } = useTranslation("identityerrors");
	const accountService = new AccountService();

	const { setAccountInfo } = useContext(AccountContext);

	const router = useRouter();

	const [errorMessage, setErrorMessage] = useState("");
	const [isLoading, setIsLoading] = useState(false);

	type Inputs = {
		usernameOrEmail: string;
		password: string;
	};

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<Inputs>({});

	const onSubmit: SubmitHandler<Inputs> = async (data: Inputs) => {
		setIsLoading(true);
		setErrorMessage("");

		try {
			const result = await accountService.loginAsync(
				data.usernameOrEmail,
				data.password
			);
			if (result.errors) {
				setErrorMessage(result.statusCode + " - " + result.errors[0]);
				return;
			}

			const jwt = result.data!.jwt;
			const refreshToken = result.data!.refreshToken;

			localStorage.setItem("_jwt", jwt);
			localStorage.setItem("_refreshToken", refreshToken);

			const roles = getRolesFromToken(jwt);
			const name = getUsernameFromToken(jwt);
			const userId = getUserIdFromToken(jwt);

			setAccountInfo!({
				jwt: jwt,
				refreshToken: refreshToken,
				roles: roles,
				name: name,
				id: userId
			});
			router.push("/");
		} catch (error) {
			setErrorMessage("Login failed - " + (error as Error).message);
		} finally {
			setIsLoading(false);
		}
	};

	return (
			<div className="w-full max-w-lg mx-auto bg-white rounded-2xl shadow-2xl p-10 mt-52 mb-10">
				<h1 className="text-3xl font-bold text-center mb-2 text-[#f0941d]">
					{tCommon("LoginTitle")}
				</h1>
				{errorMessage && (
					<div className="mb-4 text-center text-red-600 text-sm">
						{tIdentityerrors("InvalidLogin")}
					</div>
				)}
				<form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
					<div>
						<label
							htmlFor="Input_UsernameOrEmail"
							className="block text-sm font-medium text-gray-700 mb-1"
						>
							{tCommon("UsernameOrEmail")}
						</label>
						<input
							id="Input_UsernameOrEmail"
							type="text"
							autoComplete="off"
							placeholder={tCommon("UsernameOrEmail")}
							className={`form-input w-full border rounded px-3 py-2 ${
								errors.usernameOrEmail
									? "border-red-500"
									: "border-gray-300"
							}`}
							{...register("usernameOrEmail", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tCommon("UsernameOrEmail"),
									}),
								},
								minLength: {
									value: 3,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tCommon("UsernameOrEmail"),
											min: 3,
										}
									),
								},
								maxLength: {
									value: 64,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tCommon("UsernameOrEmail"),
											max: 64,
										}
									),
								},
							})}
						/>
						{errors.usernameOrEmail && (
							<p className="text-red-600 text-xs mt-1">
								{errors.usernameOrEmail.message}
							</p>
						)}
					</div>
					<div>
						<label
							htmlFor="Input_Password"
							className="block text-sm font-medium text-gray-700 mb-1"
						>
							{tCommon("Password")}
						</label>
						<input
							id="Input_Password"
							type="password"
							autoComplete="off"
							placeholder={tCommon("Password")}
							className={`form-input w-full border rounded px-3 py-2 ${
								errors.password
									? "border-red-500"
									: "border-gray-300"
							}`}
							{...register("password", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tCommon("Password"),
									}),
								},
							})}
						/>
						{errors.password && (
							<p className="text-red-600 text-xs mt-1">
								{errors.password.message}
							</p>
						)}
					</div>
					<button
						id="loginSubmit"
						type="submit"
						disabled={isLoading}
						className="w-full py-2 px-4 mt-2 bg-[#f0941d] hover:bg-[#ffa80d] text-white font-semibold rounded-lg shadow transition-colors duration-200 disabled:opacity-50"
					>
						{isLoading ? tCommon("Loading") : tCommon("LoginLink")}
					</button>
				</form>
			</div>
	);
}
