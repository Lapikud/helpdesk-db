"use client";

import Link from "next/link";
import { useContext, useEffect, useState } from "react";
import { AssetService } from "@/services/AssetService";
import { IAssetAdd } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function AssetCreate() {
	const { t: tAsset } = useTranslation("asset");
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

		if (!isAdmin) {
			router.push("/");
		}
	}, [hydrated, router, isAdmin]);

	const [errorMessage, setErrorMessage] = useState("");

	const assetService: AssetService = new AssetService();

	if (setAccountInfo) {
		assetService.injectSetAccountInfo(setAccountInfo);
	}

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<IAssetAdd>({});

	const onSubmit: SubmitHandler<IAssetAdd> = async (
		data: IAssetAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");

		try {
			const result = await assetService.addAsync({
				assetName: data.assetName,
				comment: data.comment,
				serialNumber: data.serialNumber || null,
				barcode: data.barcode || null,
			});

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/dbassets");
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
				{tAsset("AssetSingular")}
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
							htmlFor="AssetName"
						>
							{tAsset("AssetName")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="assetName"
							placeholder={tAsset("AssetNamePrompt")}
							{...register("assetName", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tAsset("AssetName"),
									}),
								},
								minLength: {
									value: 2,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tAsset("AssetName"),
											min: 2,
										}
									),
								},
								maxLength: {
									value: 128,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tAsset("AssetName"),
											max: 128,
										}
									),
								},
							})}
						/>
						{errors.assetName && (
							<span className="text-red-600 text-sm">
								{errors.assetName.message}
							</span>
						)}
					</div>
					<div className="relative">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Comment"
						>
							{tCommon("Comment")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="Comment"
							placeholder={tCommon("CommentPrompt")}
							{...register("comment", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tCommon("Comment"),
									}),
								},
								minLength: {
									value: 2,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tCommon("Comment"),
											min: 2,
										}
									),
								},
								maxLength: {
									value: 255,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tCommon("Comment"),
											max: 255,
										}
									),
								},
							})}
						/>
						{errors.comment && (
							<span className="text-red-600 text-sm">
								{errors.comment.message}
							</span>
						)}
					</div>
					<div className="relative">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="SerialNumber"
						>
							{tAsset("SerialNumber")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="SerialNumber"
							placeholder={tAsset("SerialNumberPrompt")}
							{...register("serialNumber", {
								maxLength: {
									value: 255,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tAsset("SerialNumber"),
											max: 255,
										}
									),
								},
							})}
						/>
						{errors.serialNumber && (
							<span className="text-red-600 text-sm">
								{errors.serialNumber.message}
							</span>
						)}
					</div>
					<div className="relative">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Barcode"
						>
							{tAsset("Barcode")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="Barcode"
							placeholder={tAsset("BarcodePrompt")}
							{...register("barcode", {
								maxLength: {
									value: 255,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tAsset("Barcode"),
											max: 255,
										}
									),
								},
							})}
						/>
						{errors.barcode && (
							<span className="text-red-600 text-sm">
								{errors.barcode.message}
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
				<Link
					href="/dbassets"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
