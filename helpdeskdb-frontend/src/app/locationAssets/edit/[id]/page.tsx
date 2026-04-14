"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { AssetService } from "@/services/AssetService";
import { LocationService } from "@/services/LocationService";
import { IAsset, ILocation, ILocationAsset } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function LocationAssetsEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tLocationAssets } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	const [assetsData, setAssetsData] = useState<IAsset[]>([]);
	const [categoriesData, setLocationsData] = useState<ILocation[]>([]);

	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[]
	);
	if (setAccountInfo) {
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");


	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
	} = useForm<ILocationAsset>();

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
				setIsLoading(true);

				// get this LocationAsset
				const thisLocationAssets = await locationAssetsService.getAsync(
					id
				);

				if (thisLocationAssets.errors || !thisLocationAssets.data) {
					setErrorMessage(
						thisLocationAssets.errors?.join(", ") ||
							"Failed to load location assets"
					);
					return;
				}

				// get all assets
				const assetsResult = await assetService.getAllAsync();
				if (assetsResult.errors) {
					console.log(assetsResult.errors);
					return;
				}

				// set assetsData to this current location
				setAssetsData(
					assetsResult.data!.filter(
						(u) => u.id === thisLocationAssets.data!.assetId
					)
				);

				// get all categoreis
				const categoriesResult = await locationService.getAllAsync();
				if (categoriesResult.errors) {
					console.log(categoriesResult.errors);
					return;
				}

				setLocationsData(categoriesResult.data!);

				reset({
					locationId: thisLocationAssets.data.locationId,
					assetId: thisLocationAssets.data.assetId,
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
		router,
		id,
		reset,
		isAdmin,
		assetService,
		locationService,
		locationAssetsService
	]);

	const onSubmit: SubmitHandler<ILocationAsset> = async (
		data: ILocationAsset
	) => {
		setErrorMessage("Loading...");
		try {
			const changedBy = accountInfo?.name!; //
			const result = await locationAssetsService.updateAsync({
				id: id,
				assetId: data.assetId,
				locationId: data.locationId,
				createdBy: changedBy,
			});
			console.log("edit result", result);

			if (result.errors && result.errors.length > 0) {
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
							htmlFor="As"
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
								{tCommon("SelectA")}{" "}
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
							{tCommon("EditLink")}
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
