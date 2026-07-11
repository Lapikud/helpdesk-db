"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { OverviewService } from "@/services/OverviewService";
import {
	ReadonlyURLSearchParams,
	useRouter,
	useSearchParams,
} from "next/navigation";
import {
	IAssetViewModel,
	IAssetViewModelCreate,
	IAssetViewModelRemove,
	IAssetViewModelUpdate,
} from "@/types/domain/IAssetViewModels";
import {
	ICategory,
	ILocation,
	IOwner,
	ICategoryAsset,
	ILocationAsset,
	IOwnerAsset,
	IAssetReservationWithNames,
	IAssetReservationAdd,
	IAssetReservationUpdate,
} from "@/types/domain/DomainTypes";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import AssetList from "@/components/AssetList";
import { useBarcodeScanner } from "@/hooks/useBarcodeScanner";
import { EditAssetDialog } from "@/components/dialogs/overviewDialogs/EditAssetDialog";
import { CategoryService } from "@/services/CategoryService";
import { LocationService } from "@/services/LocationService";
import { OwnerService } from "@/services/OwnerService";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { AssetService } from "@/services/AssetService";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { OwnerAssetsService } from "@/services/OwnerAssetsService";
import { AssetReservationService } from "@/services/AssetReservationService";
import { RemoveAssetDialog } from "@/components/dialogs/overviewDialogs/RemoveAssetDialog";
import Spinner from "@/components/LoadingSpinner";
import { CreateAssetDialog } from "@/components/dialogs/overviewDialogs/CreateAssetDialog";
import { AppRouterInstance } from "next/dist/shared/lib/app-router-context.shared-runtime";
import { ReserveAssetDialog } from "@/components/dialogs/overviewDialogs/ReserveAssetDialog";
import { ChangeReservationTimeDialog } from "@/components/dialogs/overviewDialogs/ChangeReservationTimeDialog";
import { RemoveReservationDialog } from "@/components/dialogs/overviewDialogs/RemoveReservationDialog";

export default function Overview() {
	const { t: tAssetViewModel } = useTranslation("assetviewmodel");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const searchParams = useSearchParams();

	const [selectedMode, setSelectedMode] = useState<string>("lines");

	const [loading, setLoading] = useState<Record<string, boolean>>({});
	const [createLoading, setCreateLoading] = useState(false);

	const [showRemoveModal, setShowRemoveModal] = useState(false);
	const [showCreateModal, setShowCreateModal] = useState(false);
	const [showUpdateModal, setShowUpdateModal] = useState(false);
	const [showReserveModal, setShowReserveModal] = useState(false);
	const [showChangeReservationModal, setShowChangeReservationModal] =
		useState(false);
	const [showRemoveReservationModal, setShowRemoveReservationModal] =
		useState(false);

	const [assetComment, setAssetComment] = useState<string>("");

	const [categories, setCategories] = useState<ICategory[]>([]);
	const [locations, setLocations] = useState<ILocation[]>([]);
	const [owners, setOwners] = useState<IOwner[]>([]);

	const [searchTerm, setSearchTerm] = useState(
		searchParams.get("searchTerm") || "",
	);
	const [searchInput, setSearchInput] = useState(searchTerm);

	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);

	const [assetToRemove, setAssetToRemove] = useState<IAssetViewModel | null>(
		null,
	);
	const [assetToUpdate, setAssetToUpdate] = useState<IAssetViewModel | null>(
		null,
	);
	const [reservationToRemove, setReservationToRemove] =
		useState<IAssetReservationWithNames | null>(null);
	const [assetToReserve, setAssetToReserve] =
		useState<IAssetViewModel | null>(null);
	const [reservationIdToChange, setReservationIdToChange] = useState<
		string | null
	>(null);
	const [initialReservationFrom, setInitialReservationFrom] =
		useState<Date | null>(null);
	const [initialReservationTo, setInitialReservationTo] =
		useState<Date | null>(null);
	const [categoryAssets, setCategoryAssets] = useState<ICategoryAsset | null>(
		null,
	);
	const [locationAssets, setLocationAssets] = useState<ILocationAsset | null>(
		null,
	);
	const [ownerAssets, setOwnerAssets] = useState<IOwnerAsset | null>(null);

	const [availableAssets, setAvailableAssets] = useState<IAssetViewModel[]>(
		[],
	);
	const [assetsReservedByUser, setAssetsReservedByUser] = useState<
		IAssetViewModel[]
	>([]);

	const overviewService: OverviewService = useMemo(
		() => new OverviewService(),
		[],
	);
	const categoryService: CategoryService = useMemo(
		() => new CategoryService(),
		[],
	);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[],
	);
	const ownerService: OwnerService = useMemo(() => new OwnerService(), []);
	const assetsService: AssetService = useMemo(() => new AssetService(), []);
	const categoryAssetsService: CategoryAssetsService = useMemo(
		() => new CategoryAssetsService(),
		[],
	);
	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[],
	);
	const ownerAssetsService: OwnerAssetsService = useMemo(
		() => new OwnerAssetsService(),
		[],
	);
	const assetReservationService: AssetReservationService = useMemo(
		() => new AssetReservationService(),
		[],
	);
	if (setAccountInfo) {
		overviewService.injectSetAccountInfo(setAccountInfo);
		categoryService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
		ownerService.injectSetAccountInfo(setAccountInfo);
		assetsService.injectSetAccountInfo(setAccountInfo);
		categoryAssetsService.injectSetAccountInfo(setAccountInfo);
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		ownerAssetsService.injectSetAccountInfo(setAccountInfo);
		assetReservationService.injectSetAccountInfo(setAccountInfo);
	}

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		const urlTerm = searchParams.get("searchTerm") || "";
		setSearchTerm(urlTerm);
		setSearchInput(urlTerm);
	}, [searchParams]);

	const fetchData = useCallback(
		async (showSpinner = false) => {
			try {
				if (showSpinner) setIsLoading(true);
				const result = await overviewService.getOverview(searchTerm);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				setAvailableAssets(result.data!.availableAssets);
				setAssetsReservedByUser(result.data!.assetsReservedByUser);
			} catch (error) {
				console.error("Error fetching data:", error);
			} finally {
				setIsLoading(false);
			}
		},
		[overviewService, searchTerm],
	);

	useEffect(() => {
		if (!hydrated) return;

		// Only show spinner on first load or if data is empty
		const shouldShowSpinner =
			availableAssets.length === 0 && assetsReservedByUser.length === 0;
		fetchData(shouldShowSpinner);
	}, [
		hydrated,
		accountInfo,
		searchTerm,
		fetchData,
		availableAssets.length,
		assetsReservedByUser.length,
	]);

	const submitSearch = useCallback(
		(value: string) => {
			const params = new URLSearchParams();
			if (value) params.set("searchTerm", value);
			router.push(`${window.location.pathname}?${params.toString()}`);
		},
		[router],
	);

	const handleSearch = (e: React.SubmitEvent) => {
		e.preventDefault();
		submitSearch(searchInput);
	};

	const anyDialogOpen =
		showRemoveModal ||
		showCreateModal ||
		showUpdateModal ||
		showReserveModal ||
		showChangeReservationModal ||
		showRemoveReservationModal;

	useBarcodeScanner({
		onScan: useCallback(
			(code: string) => {
				setSearchInput(code);
				submitSearch(code);
			},
			[submitSearch],
		),
		enabled: !anyDialogOpen,
	});

	function updateQueryParam(
		router: AppRouterInstance,
		searchParams: ReadonlyURLSearchParams,
		key: string,
		value: string | null,
	) {
		const params = new URLSearchParams(searchParams.toString());
		if (value === null) {
			params.delete(key);
		} else {
			params.set(key, value);
		}
		router.push(`${window.location.pathname}?${params.toString()}`, {
			scroll: false,
		});
	}

	const getDataCreateMenu = async () => {
		setCreateLoading(true);
		try {
			const [categoriesResponse, locationsResponse, ownersResponse] =
				await Promise.all([
					categoryService.getAllAsync(),
					locationService.getAllAsync(),
					ownerService.getAllAsync(),
				]);

			if (categoriesResponse.errors) {
				throw new Error(categoriesResponse.errors.join(", "));
			}
			setCategories(categoriesResponse.data ?? []);

			if (locationsResponse.errors) {
				throw new Error(locationsResponse.errors.join(", "));
			}
			setLocations(locationsResponse.data ?? []);

			if (ownersResponse.errors) {
				throw new Error(ownersResponse.errors.join(", "));
			}
			setOwners(ownersResponse.data ?? []);
		} catch (error) {
			console.error("Error getting asset create viewmodel:", error);
		}
		setCreateLoading(false);
	};

	const handleCreate = async (createAssetModel: IAssetViewModelCreate) => {
		setCreateLoading(true);
		try {
			const result = await overviewService.createAsset(createAssetModel);
			if (result.statusCode! >= 400) {
				alert(result.errors?.join(", ") || "Failed to create asset");
			} else {
				await fetchData();
			}
		} catch (error) {
			console.error("Error creating asset:", error);
			alert("Failed to create asset");
		} finally {
			setCreateLoading(false);
			setShowCreateModal(false);
		}
	};

	const getDataForEditMenu = async (assetId: string) => {
		setLoading((prev) => ({ ...prev, [assetId]: true }));
		try {
			const [
				assetResponse,
				categoryAssets,
				locationAssets,
				ownerAssets,
				categoriesResponse,
				locationsResponse,
				ownersResponse,
			] = await Promise.all([
				assetsService.getAsync(assetId),
				categoryAssetsService.getCategoryAssetByAssetId(assetId),
				locationAssetsService.getLocationAssetByAssetId(assetId),
				ownerAssetsService.getOwnerAssetByAssetId(assetId),
				categoryService.getAllAsync(),
				locationService.getAllAsync(),
				ownerService.getAllAsync(),
			]);

			if (assetResponse.errors)
				throw new Error(assetResponse.errors.join(", "));
			setAssetComment(assetResponse.data?.comment ?? "");

			setCategoryAssets(categoryAssets);
			setLocationAssets(locationAssets);
			setOwnerAssets(ownerAssets);

			if (categoriesResponse.errors)
				throw new Error(categoriesResponse.errors.join(", "));
			setCategories(categoriesResponse.data ?? []);

			if (locationsResponse.errors)
				throw new Error(locationsResponse.errors.join(", "));
			setLocations(locationsResponse.data ?? []);

			if (ownersResponse.errors)
				throw new Error(ownersResponse.errors.join(", "));
			setOwners(ownersResponse.data ?? []);
		} catch (error) {
			console.error("Error getting asset update viewmodel:", error);
		}
		setLoading((prev) => ({ ...prev, [assetId]: false }));
	};

	const handleEdit = async (
		assetId: string,
		updadeAssetModel: IAssetViewModelUpdate,
	) => {
		console.log(updadeAssetModel);
		setLoading((prev) => ({ ...prev, [assetId]: true }));
		try {
			const result = await overviewService.updateAsset(
				assetId,
				updadeAssetModel,
			);
			if (result.statusCode! >= 400) {
				alert(result.errors?.join(", ") || "Failed to edit asset");
			} else {
				await fetchData();
			}
		} catch (error) {
			console.error("Error editing asset:", error);
			alert("Failed to edit asset");
		} finally {
			setLoading((prev) => ({ ...prev, [assetId]: false }));
			setShowUpdateModal(false);
			setAssetToUpdate(null);
		}
	};

	const handleRemove = async (
		assetId: string,
		assetRemoveVm: IAssetViewModelRemove,
	) => {
		setLoading((prev) => ({ ...prev, [assetId]: true }));
		try {
			const result = await overviewService.removeAsset(
				assetId,
				assetRemoveVm,
			);
			if (result.statusCode! >= 400) {
				alert(result.errors?.join(", ") || "Failed to remove asset");
			} else {
				await fetchData();
			}
		} catch (error) {
			console.error("Error removing asset:", error);
			alert("Failed to remove asset");
		} finally {
			setLoading((prev) => ({ ...prev, [assetId]: false }));
			setShowRemoveModal(false);
			setAssetToRemove(null);
		}
	};

	const handleReserve = async (
		assetId: string,
		assetReservationVm: IAssetReservationAdd,
	) => {
		setLoading((prev) => ({ ...prev, [assetId]: true }));
		try {
			const result = await overviewService.reserveAsset(
				assetId,
				assetReservationVm,
			);
			if (result.statusCode! >= 400) {
				return {
					success: false,
					error:
						result.errors?.join(", ") || "Failed to reserve asset",
				};
			} else {
				await fetchData();
				return { success: true };
			}
		} catch (error) {
			console.error("Error reserving asset:", error);
			return { success: false, error: "Failed to reserve asset" };
		} finally {
			setLoading((prev) => ({ ...prev, [assetId]: false }));
		}
	};

	const handleReturnAsset = async (assetId: string) => {
		setLoading((prev) => ({ ...prev, [assetId]: true }));
		try {
			const result = await overviewService.returnAsset(assetId);
			if (result.statusCode! >= 400) {
				alert(result.errors?.join(", ") || "Failed to return asset");
			} else {
				await fetchData();
			}
		} catch (error) {
			console.error("Error returning asset:", error);
			alert("Failed to return asset");
		} finally {
			setLoading((prev) => ({ ...prev, [assetId]: false }));
		}
	};

	const handleRemoveReservation = async (assetId: string) => {
		setLoading((prev) => ({ ...prev, [assetId]: true }));
		try {
			const result = await overviewService.removeReservation(assetId);
			if (result.statusCode! >= 400) {
				alert(
					result.errors?.join(", ") || "Failed to remove reservation",
				);
			} else {
				await fetchData();
			}
		} catch (error) {
			console.error("Error removing reservation:", error);
			alert("Failed to remove reservation");
		} finally {
			setLoading((prev) => ({ ...prev, [assetId]: false }));
			setShowRemoveReservationModal(false);
			setAssetToReserve(null);
		}
	};

	const handleChangeReservationTime = async (
		assetReservationId: string,
		updateData: IAssetReservationUpdate,
	): Promise<{ error?: string } | void> => {
		setLoading((prev) => ({ ...prev, [updateData.assetId]: true }));
		try {
			const result = await overviewService.changeReservationTime(
				assetReservationId,
				updateData,
			);
			if (result.statusCode! >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update reservation time",
				};
			}
			await fetchData();
			setShowChangeReservationModal(false);
			setAssetToReserve(null);
			setReservationIdToChange(null);
		} catch (error) {
			console.error("Error updating reservation time:", error);
			return { error: (error as Error).message };
		} finally {
			setLoading((prev) => ({ ...prev, [updateData.assetId]: false }));
		}
	};

	const hasData =
		availableAssets.length > 0 || assetsReservedByUser.length > 0;

	if (!hydrated || (isLoading && !hasData)) {
		return <Spinner className="h-64" />;
	}

	return (
		<div className="min-h-screen bg-[#efefef] -mx-3 sm:-mx-4 px-6 sm:px-14 py-8 text-left">
			<h1 className="text-3xl font-bold text-[#424242] mb-6">
				{tAssetViewModel("AssetsOverview")}
			</h1>
			<AssetList
				availableAssets={availableAssets}
				assetsReservedByUser={assetsReservedByUser}
				onEditAsset={async (asset: IAssetViewModel) => {
					updateQueryParam(router, searchParams, "editId", asset.id);
					setAssetToUpdate(asset);
					await getDataForEditMenu(asset.id);
					setShowUpdateModal(true);
				}}
				onRemoveAsset={async (asset: IAssetViewModel) => {
					updateQueryParam(
						router,
						searchParams,
						"removeId",
						asset.id,
					);
					setAssetToRemove(asset);
					setShowRemoveModal(true);
				}}
				onReserveAsset={async (asset: IAssetViewModel) => {
					console.log("onReseveAsset clicked: " + asset.assetName);
					updateQueryParam(
						router,
						searchParams,
						"reserveId",
						asset.id,
					);
					setAssetToReserve(asset);
					setShowReserveModal(true);
				}}
				loading={loading}
				mode={selectedMode}
				onModeChange={(m) => setSelectedMode(m)}
				searchInput={searchInput}
				onSearchChange={(v) => setSearchInput(v)}
				onSearchSubmit={handleSearch}
				createLoading={createLoading}
				onCreateAsset={async () => {
					await getDataCreateMenu();
					setShowCreateModal(true);
				}}
				onChangeReservationTime={async (reservationId: string) => {
					setLoading((prev) => ({ ...prev, [reservationId]: true }));
					updateQueryParam(
						router,
						searchParams,
						"changeReservationId",
						reservationId,
					);
					const asset = assetsReservedByUser.find(
						(a) => a.reservationId === reservationId,
					);
					if (asset) {
						setAssetToReserve(asset);
						setReservationIdToChange(reservationId);

						try {
							const reservationResult =
								await assetReservationService.getAsync(
									reservationId,
								);
							if (reservationResult.data) {
								setInitialReservationFrom(
									new Date(
										reservationResult.data.reservationFrom,
									),
								);
								setInitialReservationTo(
									new Date(
										reservationResult.data.reservationTo,
									),
								);
								setShowChangeReservationModal(true);
							} else {
								alert("Failed to load reservation details");
							}
						} catch (e) {
							console.error(e);
							alert("Failed to load reservation details");
						}
					}
					setLoading((prev) => ({ ...prev, [reservationId]: false }));
				}}
				onReturnAsset={handleReturnAsset}
				onRemoveReservation={async (assetId: string) => {
					const asset = assetsReservedByUser.find(
						(a) => a.id === assetId,
					);
					if (asset && asset.reservationId) {
						setLoading((prev) => ({ ...prev, [assetId]: true }));
						try {
							const resInfo =
								await assetReservationService.getAsync(
									asset.reservationId,
								);
							if (resInfo.data) {
								setReservationToRemove({
									...resInfo.data,
									assetName: asset.assetName,
									userName: accountInfo?.name ?? "",
								});
								setShowRemoveReservationModal(true);
							} else {
								alert("Failed to load reservation details");
							}
						} catch (e) {
							console.error(e);
							alert("Failed to load reservation details");
						} finally {
							setLoading((prev) => ({
								...prev,
								[assetId]: false,
							}));
						}
					}
				}}
			/>
			<CreateAssetDialog
				open={showCreateModal}
				onClose={() => setShowCreateModal(false)}
				onSubmit={handleCreate}
				categories={categories}
				locations={locations}
				owners={owners}
				isLoading={false}
			/>
			<EditAssetDialog
				open={showUpdateModal}
				asset={assetToUpdate}
				comment={assetComment}
				categoryAssets={categoryAssets}
				ownerAssets={ownerAssets}
				locationAssets={locationAssets}
				categories={categories}
				owners={owners}
				locations={locations}
				onClose={() => {
					setShowUpdateModal(false);
					updateQueryParam(router, searchParams, "editId", null);
				}}
				onConfirm={handleEdit}
				isLoading={assetToUpdate ? loading[assetToUpdate.id] : false}
			/>
			<RemoveAssetDialog
				open={showRemoveModal}
				asset={assetToRemove}
				onClose={() => {
					setShowRemoveModal(false);
					updateQueryParam(router, searchParams, "removeId", null);
				}}
				onConfirm={handleRemove}
				isLoading={assetToRemove ? loading[assetToRemove.id] : false}
			/>
			<ReserveAssetDialog
				open={showReserveModal}
				asset={assetToReserve}
				onClose={() => {
					setShowReserveModal(false);
					setAssetToReserve(null);
					updateQueryParam(router, searchParams, "reserveId", null);
				}}
				onConfirm={handleReserve}
				isLoading={assetToReserve ? loading[assetToReserve.id] : false}
			/>
			<ChangeReservationTimeDialog
				open={showChangeReservationModal}
				assetReservationId={reservationIdToChange}
				asset={assetToReserve}
				initialFrom={initialReservationFrom}
				initialTo={initialReservationTo}
				onClose={() => {
					setShowChangeReservationModal(false);
					updateQueryParam(
						router,
						searchParams,
						"changeReservationId",
						null,
					);
					setInitialReservationFrom(null);
					setInitialReservationTo(null);
				}}
				onConfirm={handleChangeReservationTime}
				isLoading={assetToReserve ? loading[assetToReserve.id] : false}
			/>
			<RemoveReservationDialog
				open={showRemoveReservationModal}
				reservation={reservationToRemove}
				onClose={() => {
					setShowRemoveReservationModal(false);
					setReservationToRemove(null);
				}}
				onConfirm={handleRemoveReservation}
				isLoading={
					reservationToRemove
						? loading[reservationToRemove.assetId]
						: false
				}
			/>
		</div>
	);
}
