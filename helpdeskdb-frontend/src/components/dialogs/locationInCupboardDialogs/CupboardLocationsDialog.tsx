import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Modal } from "../common/Modal";
import { locationInCupboardService, locationService } from "@/services";
import { unwrap } from "@/services/errors";
import { qk } from "@/lib/queryKeys";
import {
	useLocations,
	useLocationsInCupboards,
} from "@/hooks/queries/entityQueries";
import {
	ILocationAdd,
	ILocationInCupboard,
	ILocationInCupboardAdd,
	ILocationInCupboardWithNames,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { locationFormConfig } from "../entityConfigs/location";
import { CreateLocationInCupboardDialog } from "./CreateLocationInCupboardDialog";
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

	const queryClient = useQueryClient();
	const enabled = open && !!cupboard;
	const locationsQuery = useLocations({ enabled });
	const linksQuery = useLocationsInCupboards({ enabled });
	const allLocations = useMemo(
		() => locationsQuery.data ?? [],
		[locationsQuery.data],
	);
	const records = useMemo(() => linksQuery.data ?? [], [linksQuery.data]);
	const listLoading = locationsQuery.isLoading || linksQuery.isLoading;
	const listError = locationsQuery.isError || linksQuery.isError;

	const invalidateLinks = () =>
		queryClient.invalidateQueries({ queryKey: qk.locationsInCupboards() });

	const createLink = useMutation({
		mutationFn: (dto: ILocationInCupboardAdd) =>
			unwrap(locationInCupboardService.addAsync(dto)),
		onSuccess: invalidateLinks,
	});
	const createLocationAndLink = useMutation({
		mutationFn: async ({
			dto,
			cupboardId,
		}: {
			dto: ILocationAdd;
			cupboardId: string;
		}) => {
			const created = await unwrap(locationService.addAsync(dto));
			try {
				return await unwrap(
					locationInCupboardService.addAsync({
						locationId: created.id,
						cupboardId,
					}),
				);
			} catch {
				throw new Error(
					"Location created but failed to add it to the cupboard",
				);
			}
		},
		onSuccess: invalidateLinks,
		// The location exists even when the link step failed, so refresh
		// qk.locations() regardless of outcome.
		onSettled: () =>
			queryClient.invalidateQueries({ queryKey: qk.locations() }),
	});
	const editLink = useMutation({
		mutationFn: (dto: ILocationInCupboard) =>
			unwrap(locationInCupboardService.updateAsync(dto)),
		onSuccess: invalidateLinks,
	});
	const deleteLink = useMutation({
		mutationFn: (id: string) =>
			unwrap(locationInCupboardService.deleteAsync(id)),
		onSuccess: invalidateLinks,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showCreateLocation, setShowCreateLocation] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);
	const [entryToEdit, setEntryToEdit] =
		useState<ILocationInCupboardWithNames | null>(null);
	const [entryToDelete, setEntryToDelete] =
		useState<ILocationInCupboardWithNames | null>(null);

	// Locations linked to *any* cupboard are unavailable, not just this one's.
	const usedLocationIds = useMemo(
		() => new Set(records.map((r) => r.locationId)),
		[records],
	);

	const entries = useMemo<ILocationInCupboardWithNames[]>(() => {
		if (!cupboard) return [];
		const locationMap = new Map(
			allLocations.map((l) => [l.id, l.locationName]),
		);
		return records
			.filter((r) => r.cupboardId === cupboard.id)
			.map((r) => ({
				...r,
				codeName: cupboard.codeName,
				locationName: locationMap.get(r.locationId) ?? r.locationId,
			}));
	}, [records, allLocations, cupboard]);

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
		try {
			await createLink.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleCreateLocation = async (dto: ILocationAdd) => {
		if (!cupboard) return;
		try {
			await createLocationAndLink.mutateAsync({
				dto,
				cupboardId: cupboard.id,
			});
			setShowCreateLocation(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ILocationInCupboard) => {
		try {
			await editLink.mutateAsync(dto);
			setShowEdit(false);
			setEntryToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteLink.mutateAsync(id);
			setShowDelete(false);
			setEntryToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
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

				{listError && (
					<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
						{tCommon("LoadFailed")}
					</div>
				)}

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
				isLoading={createLink.isPending}
			/>

			<EntityFormDialog
				open={showCreateLocation}
				mode="create"
				config={locationFormConfig}
				onClose={() => setShowCreateLocation(false)}
				onConfirm={handleCreateLocation}
				isLoading={createLocationAndLink.isPending}
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
				isLoading={editLink.isPending}
			/>

			<DeleteLocationInCupboardDialog
				open={showDelete}
				entry={entryToDelete}
				onClose={() => {
					setShowDelete(false);
					setEntryToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLink.isPending}
			/>
		</>
	);
};
