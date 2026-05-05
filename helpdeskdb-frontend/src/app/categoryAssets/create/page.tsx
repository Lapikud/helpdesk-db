"use client";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { AssetService } from "@/services/AssetService";
import { CategoryService } from "@/services/CategoryService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IAsset, ICategory, ICategoryAssetAdd } from "@/types/domain/DomainTypes";

export default function CategoryAssetsCreate() {
	const { t: tCategoryAssets } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [assetsData, setAssetsData] = useState<IAsset[]>([]);
	const [categoriesData, setCategoriesData] = useState<ICategory[]>([]);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const categoryAssetsService: CategoryAssetsService = useMemo(
		() => new CategoryAssetsService(),
		[]
	);
	const categoryService: CategoryService = useMemo(
		() => new CategoryService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		categoryAssetsService.injectSetAccountInfo(setAccountInfo);
		categoryService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		const fetchData = async () => {
			try {
				const categoryAssetsResult =
					await categoryAssetsService.getAllAsync();
				if (categoryAssetsResult.errors) {
					console.log(categoryAssetsResult.errors);
					return;
				}

				const assetsResult = await assetService.getAllAsync();
				if (assetsResult.errors) {
					console.log(assetsResult.errors);
					return;
				}

				// Filter out assets that already have a category
				const usedAssetIds = new Set(
					categoryAssetsResult.data!.map((ca) => ca.assetId)
				);
				const unusedAssets = assetsResult.data!.filter(
					(asset) => !usedAssetIds.has(asset.id)
				);
				setAssetsData(unusedAssets);

				const categorysResult = await categoryService.getAllAsync();
				if (categorysResult.errors) {
					console.log(categorysResult.errors);
					return;
				}
				setCategoriesData(categorysResult.data!);
			} catch (error) {
				console.error("error fetching data: ", error);
			}
		};
		fetchData();
	}, [hydrated, router, isAdmin, categoryAssetsService, assetService, categoryService]);

	const [errorMessage, setErrorMessage] = useState("");

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<ICategoryAssetAdd>({});

	const onSubmit: SubmitHandler<ICategoryAssetAdd> = async (
		data: ICategoryAssetAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");
		const createdBy = accountInfo?.name!;
		try {
			const result = await categoryAssetsService.addAsync({
				...data,
				createdBy,
			});

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/categoryAssets");
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
				{tCategoryAssets("CategoryAssetsSingular")}
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
							htmlFor="Asset"
						>
							{tCategoryAssets("Asset")}
						</label>
						<select
							id="assetId"
							{...register("assetId", {
								required: tValidation("Required", {
									field: tCategoryAssets("Asset"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tCategoryAssets("Asset")}
							</option>
							{assetsData.map((asset) => (
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
					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Category"
						>
							{tCategoryAssets("Category")}
						</label>
						<select
							id="categoryId"
							{...register("categoryId", {
								required: tValidation("Required", {
									field: tCategoryAssets("Category"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")}{" "}
								{tCategoryAssets("Category")}
							</option>
							{categoriesData.map((category) => (
								<option key={category.id} value={category.id}>
									{category.categoryName}
								</option>
							))}
						</select>
						{errors.categoryId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.categoryId.message}
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
					href="/categoryAssets"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
