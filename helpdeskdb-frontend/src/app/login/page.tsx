"use client";

import { AccountContext } from "@/context/AccountContext";
import { accountService } from "@/services";
import { ApiError, unwrap } from "@/services/errors";
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useContext, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";

type Inputs = {
	username: string;
	password: string;
};

export default function Login() {
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { t: tIdentityerrors } = useTranslation("identityerrors");

	const { setAccountInfo } = useContext(AccountContext);

	const router = useRouter();

	const [errorMessage, setErrorMessage] = useState("");

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<Inputs>({});

	// `unwrap` turns the IResultObject into a rejection carrying the status
	// code, which is what lets the 401-vs-everything-else split live in
	// onError. Retries are off for mutations (see providers.tsx), so a failed
	// login is never replayed.
	const login = useMutation({
		mutationFn: (data: Inputs) =>
			unwrap(accountService.loginAsync(data.username, data.password)),
		onSuccess: (identity) => {
			setAccountInfo!({
				id: identity.id,
				name: identity.username,
				roles: identity.roles,
			});
			// The query cache from any previous session is dropped by
			// QueryCacheReset, which watches the identity itself.
			router.push("/");
		},
		onError: (error) => {
			// 401 = bad credentials; anything else (network error, 5xx, IPA
			// outage) is a service problem, not the user's fault. A network
			// failure has no status, which `unwrap` normalises to 0.
			setErrorMessage(
				error instanceof ApiError && error.statusCode === 401
					? "InvalidLogin"
					: "LoginServiceUnavailable",
			);
		},
	});

	const onSubmit: SubmitHandler<Inputs> = (data) => {
		setErrorMessage("");
		login.mutate(data);
	};

	return (
		<div className="w-full max-w-lg mx-auto bg-white rounded-2xl shadow-2xl p-10 mt-52 mb-10">
			<h1 className="text-3xl font-bold text-center mb-2 text-[#f0941d]">
				{tCommon("LoginTitle")}
			</h1>
			{errorMessage && (
				<div className="mb-4 text-center text-red-600 text-sm">
					{tIdentityerrors(errorMessage)}
				</div>
			)}
			<form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
				<div>
					<label
						htmlFor="Input_Username"
						className="block text-sm font-medium text-gray-700 mb-1"
					>
						{tCommon("Username")}
					</label>
					<input
						id="Input_Username"
						type="text"
						autoComplete="off"
						placeholder={tCommon("Username")}
						className={`form-input w-full border rounded px-3 py-2 ${
							errors.username
								? "border-red-500"
								: "border-gray-300"
						}`}
						{...register("username", {
							required: {
								value: true,
								message: tValidation("Required", {
									field: tCommon("Username"),
								}),
							},
							minLength: {
								value: 3,
								message: tValidation(
									"MinLenghtValidationError",
									{
										field: tCommon("Username"),
										min: 3,
									},
								),
							},
							maxLength: {
								value: 64,
								message: tValidation(
									"MaxLengthValidationError",
									{
										field: tCommon("Username"),
										max: 64,
									},
								),
							},
						})}
					/>
					{errors.username && (
						<p className="text-red-600 text-xs mt-1">
							{errors.username.message}
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
					disabled={login.isPending}
					className="w-full py-2 px-4 mt-2 bg-[#f0941d] hover:bg-[#ffa80d] text-white font-semibold rounded-lg shadow transition-colors duration-200 disabled:opacity-50"
				>
					{login.isPending ? tCommon("Loading") : tCommon("LoginLink")}
				</button>
			</form>
		</div>
	);
}
