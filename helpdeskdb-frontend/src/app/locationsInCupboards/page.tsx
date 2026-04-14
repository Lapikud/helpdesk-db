"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { LocationInCupboardService } from "@/services/LocationInCupboardService";
import { CupboardService } from "@/services/CupboardService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { ILocationInCupboardWithNames } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { LocationService } from "@/services/LocationService";

export default function LocationsInCupboards() {
	const { t: tCupboardInLocation } = useTranslation("locationincupboard");
	const { t: tCommon } = useTranslation("common");

	const [isLoading, setIsLoading] = useState(true);

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const locationsInCupboardsService: LocationInCupboardService = useMemo(
		() => new LocationInCupboardService(),
		[]
	);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[]
	);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[]
	);
	if (setAccountInfo) {
		locationsInCupboardsService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<ILocationInCupboardWithNames[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated || !accountInfo?.jwt) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		const fetchData = async () => {
			try {
				setIsLoading(true);
				const result = await locationsInCupboardsService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const locationsInCupboardsWithNames = await Promise.all(
					result.data!.map(async (cupboardsInLocation) => {
						const location = await locationService.getAsync(
							cupboardsInLocation.locationId
						);
						const cupboard = await cupboardService.getAsync(
							cupboardsInLocation.cupboardId
						);
						const locationName =
							location.data?.locationName ??
							cupboardsInLocation.locationId;
						const codeName =
							cupboard.data?.codeName ??
							cupboardsInLocation.cupboardId;
						return {
							...cupboardsInLocation,
							locationName,
							codeName,
						};
					})
				);
				setData(locationsInCupboardsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			} finally {
				setIsLoading(false);
			}
		};

		fetchData();
	}, [hydrated, router, isAdmin, accountInfo?.jwt, cupboardService, locationService, locationsInCupboardsService]);

	if (!hydrated || isLoading) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tCupboardInLocation("LocationInCupboardTitle")}
			</h1>
			{isAdmin && (
				<p className="mb-4">
					<Link
						href="/locationsInCupboards/create"
						className="inline-block bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
					>
						{tCommon("CreateNewLink")}
					</Link>
				</p>
			)}

			<div className="w-full max-w-7xl overflow-x-auto shadow rounded-lg">
				<table className="w-full table-auto bg-white border border-gray-200 text-left">
					<thead className="bg-gray-100">
						<tr>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCupboardInLocation("Location")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCupboardInLocation("Cupboard")}
							</th>

							{isAdmin && (
								<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
									{tCommon("Actions")}
								</th>
							)}
						</tr>
					</thead>
					<tbody>
						{data.map((item) => (
							<tr key={item.id} className="hover:bg-gray-50">
								<td className="px-6 py-4 border-b">
									{item.locationName}
								</td>
								<td className="px-6 py-4 border-b">
									{item.codeName}
								</td>

								{isAdmin && (
									<td className="px-6 py-4 border-b text-blue-600 space-x-2">
										<Link
											href={`/locationsInCupboards/edit/${item.id}`}
											className="hover:underline"
										>
											{tCommon("EditLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/locationsInCupboards/details/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DetailsLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/locationsInCupboards/delete/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DeleteLink")}
										</Link>
									</td>
								)}
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</>
	);
}
