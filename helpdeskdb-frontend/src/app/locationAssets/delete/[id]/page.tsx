"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { AssetService } from "@/services/AssetService";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { LocationService } from "@/services/LocationService";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { ILocationAssetWithNames } from "@/types/domain/DomainTypes";

export default function LocationAssetsDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tLocationAssets } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<ILocationAssetWithNames>();
	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const locationService: LocationService = useMemo(() => new LocationService(), []);
	if (setAccountInfo) {
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

  		if (!isAdmin) {
			router.push("/");
		}
		const fetchData = async () => {
			try {
				const result = await locationAssetsService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const locationAssets = result.data!;

				const assetResult = (await assetService.getAsync(locationAssets.assetId));
				let assetName;
				if (assetResult.errors) {
					console.log(assetResult.errors);
					return;
				} else {
					assetName = assetResult.data?.assetName!;
				}

				const locationResult = (await locationService.getAsync(locationAssets.locationId));
				let locationName;
				if (locationResult.errors) {
					console.log(locationResult.errors);
					return;
				} else {
					locationName = locationResult.data?.locationName!;
				}

				setData({...locationAssets, assetName, locationName});
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, router, id, isAdmin, locationAssetsService, assetService, locationService]);

	const deleteConfirmed = async () => {
		try {
			const result = await locationAssetsService.deleteAsync(id);

			console.log("delete result", result);

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

	if (!hydrated || !data) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("DeleteTitle")}
			</h1>

			<h3 className="text-lg text-gray-700 mb-4">
				{tCommon("DeleteConfirmQuestion")}
			</h3>
			<div className="bg-white p-6 rounded-lg shadow-md max-w-xl mx-auto space-y-4">
				<h4 className="text-xl font-medium text-gray-800">
					{tLocationAssets("LocationAssetsSingular")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocationAssets("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.assetName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocationAssets("Location")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.locationName}</dd>
					</div>

				</dl>
				<div className="mt-6 flex items-center space-x-4 justify-center">
					<button
						onClick={() => deleteConfirmed()}
						type="button"
						title="Delete"
						className="bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-4 rounded transition"
					>
						{tCommon("DeleteButton")}
					</button>

					<span className="text-gray-400">|</span>

					<Link
						href="/locationAssets"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
