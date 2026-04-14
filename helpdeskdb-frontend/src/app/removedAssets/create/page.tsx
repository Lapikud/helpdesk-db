"use client";

import Link from "next/link";
import { useContext, useEffect, useState } from "react";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { IRemovedAssetAdd, IAsset } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { AssetService } from "@/services/AssetService";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function RemovedAssetCreate() {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [data, setData] = useState<IAsset[]>([]);

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/removedAssets");
			return;
		}

		const fetchData = async () => {
			try {
				const assetsService: AssetService = new AssetService();
				if (setAccountInfo) {
					assetsService.injectSetAccountInfo(setAccountInfo);
				}
				const result = await assetsService.getAllAsync();
				console.log(result);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				setData(result.data!);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, router, isAdmin, setAccountInfo]);

	const [errorMessage, setErrorMessage] = useState("");

	const removedAssetsService: RemovedAssetsService =
		new RemovedAssetsService();

	if (setAccountInfo) {
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
	}

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<IRemovedAssetAdd>({});

	const onSubmit: SubmitHandler<IRemovedAssetAdd> = async (
		data: IRemovedAssetAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");

		try {
			const result = await removedAssetsService.addAsync(data);

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/removedAssets");
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
				{tRemovedAssets("RemovedAssetsSingular")}
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
							htmlFor="RemovedAsset_Asset"
						>
							{tRemovedAssets("Asset")}
						</label>
						<select
							id="assetId"
							{...register("assetId", {
								required: tValidation("Required", {
									field: tRemovedAssets("Asset"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tRemovedAssets("Asset")}
							</option>
							{data.map((asset) => (
								<option key={asset.id} value={asset.id}>
									{asset.assetName}
								</option>
							))}
						</select>
						{errors.assetId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.assetId.message}
							</p>
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
					href="/removedAssets"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
