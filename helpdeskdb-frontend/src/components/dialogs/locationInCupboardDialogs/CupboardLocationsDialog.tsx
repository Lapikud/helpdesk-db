import {
	useCallback,
	useContext,
	useEffect,
	useMemo,
	useState,
} from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../common/Modal";
import { AccountContext } from "@/context/AccountContext";
import { LocationInCupboardService } from "@/services/LocationInCupboardService";
import { LocationService } from "@/services/LocationService";
import {
	ILocation,
	ILocationAdd,
	ILocationInCupboard,
	ILocationInCupboardAdd,
	ILocationInCupboardWithNames,
} from "@/types/domain/DomainTypes";
import { CreateLocationInCupboardDialog } from "./CreateLocationInCupboardDialog";
import { CreateLocationDialog } from "../locationDialogs/CreateLocationDialog";
import { EditLocationInCupboardDialog } from "./EditLocationInCupboardDialog";
import { DeleteLocationInCupboardDialog } from "./DeleteLocationInCupboardDialog";

interface CupboardLocationsDialogProps {
	open: boolean;
	cupboard: { id: string; codeName: string } | null;
	onClose: () => void;
}

export const CupboardLocationsDialog = ({
	open,
	cupboard,
	onClose,
}: CupboardLocationsDialogProps) => {
	const { t: tLocationInCupboard } = useTranslation("locationincupboard");
	const { t: tCommon } = useTranslation("common");

	const { setAccountInfo } = useContext(AccountContext);
	const locationInCupboardService: LocationInCupboardService = useMemo(
		() => new LocationInCupboardService(),
		[],
	);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[],
	);
	if (setAccountInfo) {
		locationInCupboardService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
	}

	const [entries, setEntries] = useState<ILocationInCupboardWithNames[]>([]);
	const [allLocations, setAllLocations] = useState<ILocation[]>([]);
	const [usedLocationIds, setUsedLocationIds] = useState<Set<string>>(
		new Set(),
	);
	const [listLoading, setListLoading] = useState(false);

	const [showCreate, setShowCreate] = useState(false);
	const [showCreateLocation, setShowCreateLocation] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);
	const [entryToEdit, setEntryToEdit] =
		useState<ILocationInCupboardWithNames | null>(null);
	const [entryToDelete, setEntryToDelete] =
		useState<ILocationInCupboardWithNames | null>(null);
	const [createLoading, setCreateLoading] = useState(false);
	const [createLocationLoading, setCreateLocationLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	const fetchEntries = useCallback(async () => {
		if (!cupboard) return;
		setListLoading(true);
		try {
			const [recordsResult, locationsResult] = await Promise.all([
				locationInCupboardService.getAllAsync(),
				locationService.getAllAsync(),
			]);

			const locations = locationsResult.data ?? [];
			setAllLocations(locations);

			const records = recordsResult.data ?? [];
			setUsedLocationIds(new Set(records.map((r) => r.locationId)));

			const locationMap = new Map(
				locations.map((l) => [l.id, l.locationName]),
			);
			const withNames: ILocationInCupboardWithNames[] = records
				.filter((r) => r.cupboardId === cupboard.id)
				.map((r) => ({
					...r,
					codeName: cupboard.codeName,
					locationName: locationMap.get(r.locationId) ?? r.locationId,
				}));
			setEntries(withNames);
		} finally {
			setListLoading(false);
		}
	}, [cupboard, locationInCupboardService, locationService]);

	useEffect(() => {
		if (open && cupboard) fetchEntries();
	}, [open, cupboard, fetchEntries]);

	const availableForCreate = useMemo(
		() => allLocations.filter((l) => !usedLocationIds.has(l.id)),
		[allLocations, usedLocationIds],
	);

	const availableForEdit = useMemo(
		() =>
			allLocations.filter(
				(l) =>
					!usedLocationIds.has(l.id) ||
					l.id === entryToEdit?.locationId,
			),
		[allLocations, usedLocationIds, entryToEdit],
	);

	const handleCreate = async (dto: ILocationInCupboardAdd) => {
		setCreateLoading(true);
		try {
			const result = await locationInCupboardService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create location in cupboard",
				};
			}
			await fetchEntries();
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setCreateLoading(false);
		}
	};

	const handleCreateLocation = async (dto: ILocationAdd) => {
		if (!cupboard) return;
		setCreateLocationLoading(true);
		try {
			const created = await locationService.addAsync(dto);
			if (created.errors || (created.statusCode ?? 0) >= 400 || !created.data) {
				return {
					error:
						created.errors?.join(", ") || "Failed to create location",
				};
			}
			const linked = await locationInCupboardService.addAsync({
				locationId: created.data.id,
				cupboardId: cupboard.id,
			});
			if (linked.errors || (linked.statusCode ?? 0) >= 400) {
				return {
					error:
						linked.errors?.join(", ") ||
						"Location created but failed to add it to the cupboard",
				};
			}
			await fetchEntries();
			setShowCreateLocation(false);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setCreateLocationLoading(false);
		}
	};

	const handleEdit = async (dto: ILocationInCupboard) => {
		setEditLoading(true);
		try {
			const result = await locationInCupboardService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update location in cupboard",
				};
			}
			await fetchEntries();
			setShowEdit(false);
			setEntryToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await locationInCupboardService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete location in cupboard",
				};
			}
			await fetchEntries();
			setShowDelete(false);
			setEntryToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!cupboard) return null;

	return (
		<>
			<Modal open={open} onClose={onClose}>
				<h2 className="text-xl font-bold mb-1 text-black">
					{tLocationInCupboard("LocationInCupboardTitle")}
				</h2>
				<h4 className="text-lg text-gray-700 mb-4">{cupboard.codeName}</h4>

				<div className="flex justify-end gap-2 mb-4">
					<button
						type="button"
						onClick={() => setShowCreateLocation(true)}
						className="border border-[#ff9800] text-[#f0941d] hover:bg-orange-50 font-medium px-4 py-2 rounded-full text-sm transition-colors duration-150"
					>
						{tLocationInCupboard("NewLocation")}
					</button>
					<button
						type="button"
						onClick={() => setShowCreate(true)}
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-4 py-2 rounded-full text-sm transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</button>
				</div>

				<div className="bg-[#efefef] rounded-2xl p-3 max-h-[50vh] overflow-y-auto">
					{listLoading ? (
						<div className="text-center py-6 text-gray-500">
							{tCommon("Loading")}
						</div>
					) : entries.length === 0 ? (
						<div className="text-center py-6 text-gray-500">
							{tLocationInCupboard("Location")} —
						</div>
					) : (
						<div className="flex flex-col gap-2">
							{entries.map((entry) => (
								<div
									key={entry.id}
									className="bg-white rounded-xl px-4 py-2 flex items-center justify-between gap-3"
								>
									<span className="text-sm text-black truncate">
										{entry.locationName}
									</span>
									<div className="flex gap-2 shrink-0">
										<button
											type="button"
											onClick={() => {
												setEntryToEdit(entry);
												setShowEdit(true);
											}}
											className="text-sm font-medium py-1.5 px-3 rounded-lg bg-[#e3f2fd] hover:bg-blue-100 text-[#50b3f1] transition-colors"
										>
											{tCommon("EditLink")}
										</button>
										<button
											type="button"
											onClick={() => {
												setEntryToDelete(entry);
												setShowDelete(true);
											}}
											className="text-sm font-medium py-1.5 px-3 rounded-lg bg-[#ffebee] hover:bg-red-100 text-[#ea6e6c] transition-colors"
										>
											{tCommon("DeleteLink")}
										</button>
									</div>
								</div>
							))}
						</div>
					)}
				</div>
			</Modal>

			<CreateLocationInCupboardDialog
				open={showCreate}
				cupboardId={cupboard.id}
				cupboardCodeName={cupboard.codeName}
				availableLocations={availableForCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<CreateLocationDialog
				open={showCreateLocation}
				onClose={() => setShowCreateLocation(false)}
				onConfirm={handleCreateLocation}
				isLoading={createLocationLoading}
			/>

			<EditLocationInCupboardDialog
				open={showEdit}
				entry={entryToEdit}
				cupboardCodeName={cupboard.codeName}
				availableLocations={availableForEdit}
				onClose={() => {
					setShowEdit(false);
					setEntryToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteLocationInCupboardDialog
				open={showDelete}
				entry={entryToDelete}
				onClose={() => {
					setShowDelete(false);
					setEntryToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</>
	);
};
