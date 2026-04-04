"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { UserAssetsService } from "@/services/UserAssetsService";
import { AssetService } from "@/services/AssetService";
import { UserService } from "@/services/UserService";
import { IAsset, IUser, IUserAsset } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function UserAssetsEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tUserAssets } = useTranslation("userassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	const [assetsData, setAssetsData] = useState<IAsset[]>([]);
	const [usersData, setUsersData] = useState<IUser[]>([]);
	const [updateComment, setUpdateComment] = useState("");
	const [commentError, setCommentError] = useState<string | null>(null);

	const userAssetsService: UserAssetsService = useMemo(
		() => new UserAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const userService: UserService = useMemo(
		() => new UserService(),
		[]
	);
	if (setAccountInfo) {
		userAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
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
	} = useForm<IUserAsset>();

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

				// get this UserAsset
				const thisUserAssets = await userAssetsService.getAsync(
					id
				);

				if (thisUserAssets.errors || !thisUserAssets.data) {
					setErrorMessage(
						thisUserAssets.errors?.join(", ") ||
							"Failed to load user assets"
					);
					return;
				}


				const userAssetsResult =
					await userAssetsService.getAllAsync();
				if (userAssetsResult.errors) {
					console.log(userAssetsResult.errors);
					return;
				}

				const assetsResult = await assetService.getAllAsync();
				if (assetsResult.errors) {
					console.log(assetsResult.errors);
					return;
				}

				// Filter out assets that already have a user
				const usedAssetIds = new Set(
					userAssetsResult.data!.map((ca) => ca.assetId)
				);
				const unusedAssets = assetsResult.data!.filter(
					(asset) => !usedAssetIds.has(asset.id) || asset.id === thisUserAssets.data!.assetId
				);
				setAssetsData(unusedAssets);

				// get all users
				const usersResult = await userService.getAllAsync();
				if (usersResult.errors) {
					console.log(usersResult.errors);
					return;
				}

				setUsersData(usersResult.data!);
				setUpdateComment(thisUserAssets.data.comment!);

				reset({
					userId: thisUserAssets.data.userId,
					assetId: thisUserAssets.data.assetId,
					comment: thisUserAssets.data.comment
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
		userService,
	]);

	const onSubmit: SubmitHandler<IUserAsset> = async (
		data: IUserAsset
	) => {
		setErrorMessage("Loading...");
		try {
			const changedBy = accountInfo?.name!; //
			const result = await userAssetsService.updateAsync({
				id: id,
				assetId: data.assetId,
				userId: data.userId,
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
				router.push("/userAssets");
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
				{tUserAssets("UserAssetSingular")}
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
							{tUserAssets("Asset")}
						</label>
						<select
							id="assetId"
							{...register("assetId", {
								required: tValidation("Required", {
									field: tUserAssets("Asset"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tUserAssets("Asset")}
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
							{tUserAssets("User")}
						</label>
						<select
							id="userId"
							{...register("userId", {
								required: tValidation("Required", {
									field: tUserAssets("User"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectA")}{" "}
								{tUserAssets("User")}
							</option>
							{usersData.map((user) => (
								<option key={user.id} value={user.id}>
									{user.userName}
								</option>
							))}
						</select>
						{errors.userId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.userId.message}
							</p>
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
					href="/userAssets"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
