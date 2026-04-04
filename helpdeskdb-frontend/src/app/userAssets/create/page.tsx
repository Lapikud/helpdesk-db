"use client";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { UserAssetsService } from "@/services/UserAssetsService";
import { AssetService } from "@/services/AssetService";
import { UserService } from "@/services/UserService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IAsset, IUser, IUserAssetAdd } from "@/types/domain/DomainTypes";

export default function UserAssetsCreate() {
	const { t: tUserAssets } = useTranslation("userassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [assetsData, setAssetsData] = useState<IAsset[]>([]);
	const [categoriesData, setCategoriesData] = useState<IUser[]>([]);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const userAssetsService: UserAssetsService = useMemo(
		() => new UserAssetsService(),
		[]
	);
	const userService: UserService = useMemo(
		() => new UserService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		userAssetsService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}

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
					(asset) => !usedAssetIds.has(asset.id)
				);
				setAssetsData(unusedAssets);

				const usersResult = await userService.getAllAsync();
				if (usersResult.errors) {
					console.log(usersResult.errors);
					return;
				}
				setCategoriesData(usersResult.data!);
			} catch (error) {
				console.error("error fetching data: ", error);
			}
		};
		fetchData();
	}, [hydrated, accountInfo, router, isAdmin, setAccountInfo]);

	const [errorMessage, setErrorMessage] = useState("");

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<IUserAssetAdd>({});

	const onSubmit: SubmitHandler<IUserAssetAdd> = async (
		data: IUserAssetAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");
		try {
			const result = await userAssetsService.addAsync(data);

			if (result.errors) {
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
							htmlFor="User"
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
								{tCommon("SelectAn")}{" "}
								{tUserAssets("User")}
							</option>
							{categoriesData.map((user) => (
								<option key={user.id} value={user.id}>
									{user.username}
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
							{tCommon("CreateButton")}
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
