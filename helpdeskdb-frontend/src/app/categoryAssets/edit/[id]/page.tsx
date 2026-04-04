"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { AssetService } from "@/services/AssetService";
import { CategoryService } from "@/services/CategoryService";
import { IAsset, ICategory, ICategoryAsset } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function CategoryAssetsEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCategoryAssets } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	const [assetsData, setAssetsData] = useState<IAsset[]>([]);
	const [categoriesData, setCategorysData] = useState<ICategory[]>([]);
	const [updateComment, setUpdateComment] = useState("");
	const [commentError, setCommentError] = useState<string | null>(null);

	const categoryAssetsService: CategoryAssetsService = useMemo(
		() => new CategoryAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const categoryService: CategoryService = useMemo(
		() => new CategoryService(),
		[]
	);
	if (setAccountInfo) {
		categoryAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		categoryService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	const validateComment = (comment: string) => {
		if (!comment || comment.trim().length === 0) {
			setCommentError(
				tValidation("Required", { field: tCommon("Comment") })
			);
			return false;
		}
		if (comment.length < 2) {
			setCommentError(
				tValidation("MinLenghtValidationError", {
					field: tCommon("Comment"),
					min: 2,
				})
			);
			return false;
		}
		if (comment.length > 255) {
			setCommentError(
				tValidation("MaxLengthValidationError", {
					field: tCommon("Comment"),
					max: 255,
				})
			);
			return false;
		}
		setCommentError(null);
		return true;
	};

	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		setValue,
	} = useForm<ICategoryAsset>();

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
			return;
		}

		const fetchData = async () => {
			try {
				setIsLoading(true);

				// get this CategoryAsset
				const thisCategoryAssets = await categoryAssetsService.getAsync(
					id
				);

				if (thisCategoryAssets.errors || !thisCategoryAssets.data) {
					setErrorMessage(
						thisCategoryAssets.errors?.join(", ") ||
							"Failed to load category assets"
					);
					return;
				}

				// get all assets
				const assetsResult = await assetService.getAllAsync();
				if (assetsResult.errors) {
					console.log(assetsResult.errors);
					return;
				}

				// set assetsData to this current category
				setAssetsData(
					assetsResult.data!.filter(
						(u) => u.id === thisCategoryAssets.data!.assetId
					)
				);

				// get all categoreis
				const categoriesResult = await categoryService.getAllAsync();
				if (categoriesResult.errors) {
					console.log(categoriesResult.errors);
					return;
				}

				setCategorysData(categoriesResult.data!);
				setUpdateComment(thisCategoryAssets.data.comment!);

				reset({
					categoryId: thisCategoryAssets.data.categoryId,
					assetId: thisCategoryAssets.data.assetId,
					comment: thisCategoryAssets.data.comment
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
		assetService,
		categoryService,
	]);

	const onSubmit: SubmitHandler<ICategoryAsset> = async (
		data: ICategoryAsset
	) => {
		setErrorMessage("Loading...");
		try {
			const changedBy = accountInfo?.name!; //
			const result = await categoryAssetsService.updateAsync({
				id: id,
				assetId: data.assetId,
				categoryId: data.categoryId,
				createdBy: changedBy,
				comment: updateComment,
			});
			console.log("edit result", result);

			if (result.errors && result.errors.length > 0) {
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

	if (!hydrated || isLoading) {
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
							htmlFor="As"
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
								{tCommon("SelectA")}{" "}
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
							value={updateComment}
							onChange={(e) => setUpdateComment(e.target.value)}
							onBlur={() => validateComment(updateComment)}
							placeholder={tCommon("CommentPrompt")}
							maxLength={255}
						/>
						{commentError && (
							<span className="text-red-600 text-sm">
								{commentError}
							</span>
						)}
					</div>

					<div>
						<button
							type="submit"
							className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition"
						>
							{tCommon("EditLink")}
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
