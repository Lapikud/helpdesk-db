"use client";

import Link from "next/link";
import { use, useContext, useEffect, useState } from "react";
import { CupboardService } from "@/services/CupboardService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function CupboardEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCupboard } = useTranslation("cupboard");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const cupboardService = new CupboardService();

	if (setAccountInfo) {
		cupboardService.injectSetAccountInfo(setAccountInfo);
	}

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
		}
	}, [hydrated, accountInfo, router, isAdmin]);

	type Inputs = {
		codeName: string;
	};

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<Inputs>({
		defaultValues: async () => {
			const result = await cupboardService.getAsync(id);
			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return { codeName: ""};
			} else {
				setErrorMessage("");
				return {
					codeName: result.data!.codeName,
				};
			}
		},
	});

	const onSubmit: SubmitHandler<Inputs> = async (data: Inputs) => {
		setErrorMessage("Loading...");
		try {
			const result = await cupboardService.updateAsync({
				id: id,
				codeName: data.codeName,
			});
			console.log("edit result", result);

			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/cupboards");
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
				{tCommon("EditTitle")}
			</h1>
			<h4 className="text-xl text-gray-700 mb-4">
				{tCupboard("CupboardSingular")}
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
							htmlFor="CodeName"
						>
							{tCupboard("CodeName")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="codeName"
							placeholder={tCupboard("CodeNamePrompt")}
							{...register("codeName", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tCupboard("CodeName"),
									}),
								},
								minLength: {
									value: 2,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tCupboard("CodeName"),
											min: 2,
										}
									),
								},
								maxLength: {
									value: 128,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tCupboard("CodeName"),
											max: 128,
										}
									),
								},
							})}
						/>
						{errors.codeName && (
							<span className="text-red-600 text-sm">
								{errors.codeName.message}
							</span>
						)}
					</div>

					<div>
						<button
							type="submit"
							value="Save"
							className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition"
						>
							{tCommon("SaveButton")}
						</button>
					</div>
				</form>
			</div>

			<div className="mt-6 text-center">
				<Link
					href="/cupboards"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
