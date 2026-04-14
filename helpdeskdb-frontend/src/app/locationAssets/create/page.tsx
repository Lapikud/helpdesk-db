"use client";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { AssetService } from "@/services/AssetService";
import { LocationService } from "@/services/LocationService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IAsset, ILocation, ILocationAssetAdd } from "@/types/domain/DomainTypes";

export default function LocationAssetsCreate() {
	const { t: tLocationAssets } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [assetsData, setAssetsData] = useState<IAsset[]>([]);
	const [categoriesData, setCategoriesData] = useState<ILocation[]>([]);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[]
	);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
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
				const locationAssetsResult =
					await locationAssetsService.getAllAsync();
				if (locationAssetsResult.errors) {
					console.log(locationAssetsResult.errors);
					return;
				}

				const assetsResult = await assetService.getAllAsync();
				if (assetsResult.errors) {
					console.log(assetsResult.errors);
					return;
				}

				// Filter out assets that already have a location
				const usedAssetIds = new Set(
					locationAssetsResult.data!.map((ca) => ca.assetId)
				);
				const unusedAssets = assetsResult.data!.filter(
					(asset) => !usedAssetIds.has(asset.id)
				);
				setAssetsData(unusedAssets);

				const locationsResult = await locationService.getAllAsync();
				if (locationsResult.errors) {
					console.log(locationsResult.errors);
					return;
				}
				setCategoriesData(locationsResult.data!);
			} catch (error) {
				console.error("error fetching data: ", error);
			}
		};
		fetchData();
	}, [hydrated, router, isAdmin, locationAssetsService, assetService, locationService]);

	const [errorMessage, setErrorMessage] = useState("");

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<ILocationAssetAdd>({});

	const onSubmit: SubmitHandler<ILocationAssetAdd> = async (
		data: ILocationAssetAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");
		const createdBy = accountInfo?.name!;
		try {
			const result = await locationAssetsService.addAsync({
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
				router.push("/locationAssets");
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
				{tLocationAssets("LocationAssetsSingular")}
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
							{tLocationAssets("Asset")}
						</label>
						<select
							id="assetId"
							{...register("assetId", {
								required: tValidation("Required", {
									field: tLocationAssets("Asset"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tLocationAssets("Asset")}
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
							htmlFor="Location"
						>
							{tLocationAssets("Location")}
						</label>
						<select
							id="locationId"
							{...register("locationId", {
								required: tValidation("Required", {
									field: tLocationAssets("Location"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")}{" "}
								{tLocationAssets("Location")}
							</option>
							{categoriesData.map((location) => (
								<option key={location.id} value={location.id}>
									{location.locationName}
								</option>
							))}
						</select>
						{errors.locationId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.locationId.message}
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
					href="/locationAssets"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
