"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { AssetReservationService } from "@/services/AssetReservationService";
import { AssetService } from "@/services/AssetService";
import { UserService } from "@/services/UserService";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IAssetReservationWithNames } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";

export default function AssetReservations() {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const assetReservationService: AssetReservationService = useMemo(
		() => new AssetReservationService(),
		[],
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const userService: UserService = useMemo(() => new UserService(), []);
	const removedAssetsService: RemovedAssetsService = useMemo(
		() => new RemovedAssetsService(),
		[],
	);

	if (setAccountInfo) {
		assetReservationService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
	}

	const [data, setData] = useState<IAssetReservationWithNames[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins") ?? false;
	const isMember = accountInfo?.roles?.includes("members") ?? false;
	const isPixel = accountInfo?.roles?.includes("pixels") ?? false;

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		const fetchData = async () => {
			try {
				const result = await assetReservationService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}

				// We need to fetch the related names and removed status.
				if (result.data) {
					const [assetsResult, usersResult, removedResult] =
						await Promise.all([
							assetService.getAllAsync(true),
							userService.getAllAsync(),
							removedAssetsService.getAllAsync(),
						]);

					const defaultAssets = assetsResult.data || [];
					const defaultUsers = usersResult.data || [];
					const removedAssets = removedResult.data || [];

					const enrichedData: IAssetReservationWithNames[] =
						result.data.map((ar) => {
							const matchedAsset = defaultAssets.find(
								(a) => a.id === ar.assetId,
							);
							const matchedUser = defaultUsers.find(
								(u) => u.id === ar.userId,
							);
							const isRemoved = removedAssets.some(
								(ra) => ra.assetId === ar.assetId,
							);

							return {
								...ar,
								assetName: matchedAsset
									? matchedAsset.assetName
									: "Unknown Asset",
								userName: matchedUser
									? matchedUser.username
									: "Unknown User",
								isRemoved: isRemoved,
							};
						});

					// Sort by reservationFrom descending
					enrichedData.sort(
						(a, b) =>
							new Date(b.reservationFrom).getTime() -
							new Date(a.reservationFrom).getTime(),
					);

					setData(enrichedData);
				}
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [
		hydrated,
		assetReservationService,
		assetService,
		userService,
		removedAssetsService,
	]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tAssetReservation("AssetReservations")}
			</h1>
			{isAdmin && (
				<p className="mb-4">
					<Link
						href="/assetReservations/create"
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
								{tAssetReservation("AssetId")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tAssetReservation("UserId")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tAssetReservation("ReservationFrom")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tAssetReservation("ReservationTo")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tAssetReservation("IsReturned")}
							</th>
							{(isAdmin || isMember || isPixel) && (
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
									{item.assetName}
								</td>
								<td className="px-6 py-4 border-b">
									{item.userName}
								</td>
								<td className="px-6 py-4 border-b">
									{new Date(
										item.reservationFrom,
									).toLocaleString()}
								</td>
								<td className="px-6 py-4 border-b">
									{new Date(
										item.reservationTo,
									).toLocaleString()}
								</td>
								<td className="px-6 py-4 border-b">
									{item.isReturned
										? tAssetReservation("Yes")
										: tAssetReservation("No")}
								</td>
								{(isAdmin || isMember || isPixel) && (
									<td className="px-6 py-4 border-b space-x-2">
										{item.isRemoved ? (
											<span className="text-red-600">
												{tAssetReservation("Removed")}
											</span>
										) : new Date(item.reservationTo) <
										  new Date() ? (
											<span className="text-red-600">
												{tAssetReservation("Expired")}
											</span>
										) : item.userId === accountInfo?.id ? (
											<span className="text-blue-600 space-x-2">
												<Link
													href={`/assetReservations/edit/${item.id}`}
													className="hover:underline"
												>
													{tCommon("EditLink")}
												</Link>
												<span className="text-gray-400">
													|
												</span>
												<Link
													href={`/assetReservations/delete/${item.id}`}
													className="hover:underline"
												>
													{tCommon("DeleteLink")}
												</Link>
											</span>
										) : null}
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
