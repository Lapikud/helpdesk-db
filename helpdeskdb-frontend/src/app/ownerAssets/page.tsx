"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { OwnerAssetsService } from "@/services/OwnerAssetsService";
import { OwnerService } from "@/services/OwnerService";
import { AssetService } from "@/services/AssetService";
import { useRouter } from "next/navigation";

import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import {
	IAsset,
	IOwner,
	IOwnerAssetAdd,
	IOwnerAssetWithNames,
} from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateOwnerAssetDialog } from "@/components/dialogs/ownerAssetDialogs/CreateOwnerAssetDialog";
import { EditOwnerAssetDialog } from "@/components/dialogs/ownerAssetDialogs/EditOwnerAssetDialog";
import { DeleteOwnerAssetDialog } from "@/components/dialogs/ownerAssetDialogs/DeleteOwnerAssetDialog";

export default function OwnerAssets() {
	const { t: tOwnerAssets } = useTranslation("ownerassets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const ownerAssetsService: OwnerAssetsService = useMemo(
		() => new OwnerAssetsService(),
		[],
	);
	const ownerService: OwnerService = useMemo(() => new OwnerService(), []);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		ownerAssetsService.injectSetAccountInfo(setAccountInfo);
		ownerService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IOwnerAssetWithNames[]>([]);
	const [allAssets, setAllAssets] = useState<IAsset[]>([]);
	const [allOwners, setAllOwners] = useState<IOwner[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [itemToEdit, setItemToEdit] = useState<IOwnerAssetWithNames | null>(
		null,
	);
	const [itemToDelete, setItemToDelete] =
		useState<IOwnerAssetWithNames | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const [ownerAssetsResult, assetsResult, ownersResult] =
			await Promise.all([
				ownerAssetsService.getAllAsync(),
				assetService.getAllAsync(true),
				ownerService.getAllAsync(),
			]);

		if (
			ownerAssetsResult.errors ||
			!ownerAssetsResult.data ||
			assetsResult.errors ||
			!assetsResult.data ||
			ownersResult.errors ||
			!ownersResult.data
		) {
			return;
		}

		const assetById = new Map(assetsResult.data.map((a) => [a.id, a]));
		const ownerById = new Map(ownersResult.data.map((o) => [o.id, o]));

		const withNames: IOwnerAssetWithNames[] = ownerAssetsResult.data.map(
			(oa) => ({
				...oa,
				assetName: assetById.get(oa.assetId)?.assetName ?? oa.assetId,
				ownerName: ownerById.get(oa.ownerId)?.ownerName ?? oa.ownerId,
			}),
		);

		setData(withNames);
		setAllAssets(assetsResult.data);
		setAllOwners(ownersResult.data);
	}, [ownerAssetsService, assetService, ownerService]);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [hydrated, router, isAdmin, fetchData]);

	const unusedAssets = useMemo(() => {
		const used = new Set(data.map((oa) => oa.assetId));
		return allAssets.filter((a) => !used.has(a.id));
	}, [allAssets, data]);

	const handleCreate = async (dto: IOwnerAssetAdd) => {
		setCreateLoading(true);
		try {
			const result = await ownerAssetsService.addAsync({
				...dto,
				createdBy: accountInfo?.name ?? "",
			});
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create owner asset",
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

	const handleEdit = async (dto: IOwnerAssetWithNames) => {
		setEditLoading(true);
		try {
			const result = await ownerAssetsService.updateAsync({
				id: dto.id,
				assetId: dto.assetId,
				ownerId: dto.ownerId,
				createdBy: accountInfo?.name ?? dto.createdBy,
			});
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update owner asset",
				};
			}
			await fetchData();
			setShowEdit(false);
			setItemToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await ownerAssetsService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete owner asset",
				};
			}
			await fetchData();
			setShowDelete(false);
			setItemToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [
				tOwnerAssets("Asset"),
				tOwnerAssets("Owner"),
				tCommon("CreatedBy"),
				tCommon("Actions"),
			]
		: [tOwnerAssets("Asset"), tOwnerAssets("Owner"), tCommon("CreatedBy")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.ownerName,
			item.createdBy || "-",
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setItemToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setItemToDelete(item);
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
			title={tOwnerAssets("OwnerAssetsTitle")}
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
			<DataTable columns={columns} rows={rows} />

			<CreateOwnerAssetDialog
				open={showCreate}
				assets={unusedAssets}
				owners={allOwners}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditOwnerAssetDialog
				open={showEdit}
				ownerAsset={itemToEdit}
				owners={allOwners}
				onClose={() => {
					setShowEdit(false);
					setItemToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteOwnerAssetDialog
				open={showDelete}
				ownerAsset={itemToDelete}
				onClose={() => {
					setShowDelete(false);
					setItemToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
