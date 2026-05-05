"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { AssetService } from "@/services/AssetService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { IAsset } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function RemovedAssetEditClient({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<IAsset[]>([]);
	const [isLoading, setIsLoading] = useState(true);
	const [removedBy, setRemovedBy] = useState<string>("");

	const removedAssetsService: RemovedAssetsService = useMemo(
		() => new RemovedAssetsService(),
		[]
	);
	const assetsService: AssetService = useMemo(() => new AssetService(), []);

	if (setAccountInfo) {
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
		assetsService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	type Inputs = {
		assetId: string;
		comment: string;
	};

	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		setValue,
	} = useForm<Inputs>();

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
				setIsLoading(true);

				const thisRemovedAsset = await removedAssetsService.getAsync(
					id
				);
				if (thisRemovedAsset.errors || !thisRemovedAsset.data) {
					setErrorMessage(
						thisRemovedAsset.errors?.join(", ") ||
							"Failed to load removed asset"
					);
					return;
				}
				setRemovedBy(thisRemovedAsset.data.removedBy);

				const currentId = thisRemovedAsset.data.assetId;
				setValue("assetId", currentId);

				const result = await assetsService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}

				const currentAsset = await assetsService.getAsync(currentId);
				const assets = [...(result.data ?? []), currentAsset.data!];
				setData(assets);

				reset({
					assetId: currentId,
					comment: thisRemovedAsset.data.comment || "",
				});
			} catch (error) {
				console.error("Error fetching data:", error);
				setErrorMessage("Failed to load data");
			} finally {
				setIsLoading(false);
			}
		};

		fetchData();
	}, [
		hydrated,
		id,
		router,
		reset,
		setValue,
		assetsService,
		isAdmin,
		removedAssetsService,
	]);

	const onSubmit: SubmitHandler<Inputs> = async (data: Inputs) => {
		setErrorMessage("Loading...");
		try {
			const result = await removedAssetsService.updateAsync({
				id: id,
				assetId: data.assetId,
				comment: data.comment,
				removedBy: removedBy,
			});
			console.log("edit result", result);

			if (result.errors && result.errors.length > 0) {
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
					href="/removedAssets"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
