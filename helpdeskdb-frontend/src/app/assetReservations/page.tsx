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
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";

export default function AssetReservations() {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const assetReservationService = useMemo(() => new AssetReservationService(), []);
	const assetService = useMemo(() => new AssetService(), []);
	const userService = useMemo(() => new UserService(), []);
	const removedAssetsService = useMemo(() => new RemovedAssetsService(), []);

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
	const showActions = isAdmin || isMember || isPixel;

	useEffect(() => { setHydrated(true); }, []);

	useEffect(() => {
		if (!hydrated) return;
		const fetchData = async () => {
			try {
				const result = await assetReservationService.getAllAsync();
				if (result.errors) return;
				if (result.data) {
					const [assetsResult, usersResult, removedResult] = await Promise.all([
						assetService.getAllAsync(true),
						userService.getAllAsync(),
						removedAssetsService.getAllAsync(),
					]);
					const enrichedData: IAssetReservationWithNames[] = result.data.map((ar) => ({
						...ar,
						assetName: assetsResult.data?.find((a) => a.id === ar.assetId)?.assetName ?? "Unknown Asset",
						userName: usersResult.data?.find((u) => u.id === ar.userId)?.username ?? "Unknown User",
						isRemoved: removedResult.data?.some((ra) => ra.assetId === ar.assetId) ?? false,
					}));
					enrichedData.sort(
						(a, b) => new Date(b.reservationTo).getTime() - new Date(a.reservationTo).getTime(),
					);
					setData(enrichedData);
				}
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, assetReservationService, assetService, userService, removedAssetsService]);

	if (!hydrated) return <Spinner className="h-64" />;

	const renderActionCell = (item: IAssetReservationWithNames) => {
		if (item.isRemoved) {
			return (
				<span className="text-sm font-medium text-red-600">
					{tAssetReservation("Removed")}
				</span>
			);
		}
		if (new Date(item.reservationTo) < new Date()) {
			return (
				<span className="text-sm font-medium text-[#c50000]">
					{tAssetReservation("Expired")}
				</span>
			);
		}
		if (item.userId === accountInfo?.id) {
			return (
				<>
					<EditButton href={`/assetReservations/edit/${item.id}`} label={tCommon("EditLink")} />
					<DeleteButton href={`/assetReservations/delete/${item.id}`} label={tCommon("DeleteLink")} />
				</>
			);
		}
		return null;
	};

	const columns = showActions
		? [
				tAssetReservation("AssetId"),
				tAssetReservation("UserId"),
				tAssetReservation("ReservationFrom"),
				tAssetReservation("ReservationTo"),
				tAssetReservation("IsReturned"),
				tCommon("Actions"),
			]
		: [
				tAssetReservation("AssetId"),
				tAssetReservation("UserId"),
				tAssetReservation("ReservationFrom"),
				tAssetReservation("ReservationTo"),
				tAssetReservation("IsReturned"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.userName,
			new Date(item.reservationFrom).toLocaleString(),
			new Date(item.reservationTo).toLocaleString(),
			item.isReturned ? tAssetReservation("Yes") : tAssetReservation("No"),
			...(showActions
				? [<ActionCell key="actions">{renderActionCell(item)}</ActionCell>]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tAssetReservation("AssetReservations")}
			createButton={
				isAdmin && (
					<Link
						href="/assetReservations/create"
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</Link>
				)
			}
		>
			<DataTable columns={columns} rows={rows} minWidth="min-w-[700px]" />
		</ListPageWrapper>
	);
}
