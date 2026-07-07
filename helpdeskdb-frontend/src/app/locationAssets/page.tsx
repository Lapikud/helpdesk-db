"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { LocationService } from "@/services/LocationService";
import { AssetService } from "@/services/AssetService";
import {
	useCallback,
	useContext,
	useEffect,
	useMemo,
	useState,
} from "react";
import {
	IAsset,
	ILocation,
	ILocationAsset,
	ILocationAssetWithNames,
} from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { CreateLocationAssetDialog } from "@/components/dialogs/locationAssetDialogs/CreateLocationAssetDialog";
import { EditLocationAssetDialog } from "@/components/dialogs/locationAssetDialogs/EditLocationAssetDialog";
import { DeleteLocationAssetDialog } from "@/components/dialogs/locationAssetDialogs/DeleteLocationAssetDialog";

export default function LocationAssets() {
	const { t: tLocationAssets } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[],
	);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[],
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}

	const [data, setData] = useState<ILocationAssetWithNames[]>([]);
	const [allAssets, setAllAssets] = useState<IAsset[]>([]);
	const [locations, setLocations] = useState<ILocation[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [locationAssetToEdit, setLocationAssetToEdit] =
		useState<ILocationAssetWithNames | null>(null);
	const [locationAssetToDelete, setLocationAssetToDelete] =
		useState<ILocationAssetWithNames | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const [laResult, assetsResult, locationsResult] = await Promise.all([
			locationAssetsService.getAllAsync(),
			assetService.getAllAsync(),
			locationService.getAllAsync(),
		]);

		const assets = assetsResult.data ?? [];
		const locationList = locationsResult.data ?? [];
		setAllAssets(assets);
		setLocations(locationList);

		const assetMap = new Map(assets.map((a) => [a.id, a.assetName]));
		const locationMap = new Map(
			locationList.map((l) => [l.id, l.locationName]),
		);

		const records = laResult.data ?? [];
		setData(
			records.map((r) => ({
				...r,
				assetName: assetMap.get(r.assetId) ?? r.assetId,
				locationName: locationMap.get(r.locationId) ?? r.locationId,
			})),
		);
	}, [locationAssetsService, assetService, locationService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const availableAssets = useMemo(() => {
		const used = new Set(data.map((d) => d.assetId));
		return allAssets.filter((a) => !used.has(a.id));
	}, [data, allAssets]);

	const assetsForEdit = useMemo(() => {
		if (!locationAssetToEdit) return availableAssets;
		const current = allAssets.find(
			(a) => a.id === locationAssetToEdit.assetId,
		);
		if (!current || availableAssets.some((a) => a.id === current.id)) {
			return availableAssets;
		}
		return [current, ...availableAssets];
	}, [availableAssets, allAssets, locationAssetToEdit]);

	const handleCreate = async (dto: { assetId: string; locationId: string }) => {
		setCreateLoading(true);
		try {
			const result = await locationAssetsService.addAsync({
				...dto,
				createdBy: accountInfo?.name ?? "",
			});
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create location asset",
				};
			}
			await fetchData();
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setCreateLoading(false);
		}
	};

	const handleEdit = async (dto: ILocationAsset) => {
		setEditLoading(true);
		try {
			const result = await locationAssetsService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update location asset",
				};
			}
			await fetchData();
			setShowEdit(false);
			setLocationAssetToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await locationAssetsService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete location asset",
				};
			}
			await fetchData();
			setShowDelete(false);
			setLocationAssetToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [
				tLocationAssets("Asset"),
				tLocationAssets("Location"),
				tCommon("CreatedBy"),
				tCommon("Actions"),
			]
		: [
				tLocationAssets("Asset"),
				tLocationAssets("Location"),
				tCommon("CreatedBy"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.locationName,
			item.createdBy,
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setLocationAssetToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setLocationAssetToDelete(item);
									setShowDelete(true);
								}}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tLocationAssets("LocationAssetsTitle")}
			createButton={
				isAdmin && (
					<button
						type="button"
						onClick={() => setShowCreate(true)}
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</button>
				)
			}
		>
			<DataTable columns={columns} rows={rows} minWidth="min-w-[600px]" />

			<CreateLocationAssetDialog
				open={showCreate}
				assets={availableAssets}
				locations={locations}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditLocationAssetDialog
				open={showEdit}
				locationAsset={locationAssetToEdit}
				assets={assetsForEdit}
				locations={locations}
				onClose={() => {
					setShowEdit(false);
					setLocationAssetToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteLocationAssetDialog
				open={showDelete}
				locationAsset={locationAssetToDelete}
				onClose={() => {
					setShowDelete(false);
					setLocationAssetToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
