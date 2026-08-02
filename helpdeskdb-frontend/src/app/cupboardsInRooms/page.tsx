"use client";

import { useTranslation } from "react-i18next";
import { cupboardService, cupboardsInRoomsService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
	useCupboards,
	useCupboardsInRooms,
	useRooms,
} from "@/hooks/queries/entityQueries";
import { makeConfirmHandler, useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	ICupboardAdd,
	ICupboardInRoom,
	ICupboardInRoomAdd,
	ICupboardInRoomWithNames,
} from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import ErrorBanner from "@/components/ErrorBanner";
import CreateButton from "@/components/CreateButton";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { EntityFormDialog } from "@/components/dialogs/common/EntityFormDialog";
import { EntityEditDialog } from "@/components/dialogs/common/EntityEditDialog";
import { EntityDeleteDialog } from "@/components/dialogs/common/EntityDeleteDialog";
import { cupboardFormConfig } from "@/components/dialogs/entityConfigs/cupboard";
import {
	cupboardInRoomCreateConfig,
	cupboardInRoomDeleteSummary,
	cupboardInRoomEditConfig,
	cupboardInRoomToForm,
	cupboardInRoomToUpdate,
	cupboardsToOptions,
	roomsToOptions,
} from "@/components/dialogs/entityConfigs/cupboardInRoom";
import { CupboardLocationsDialog } from "@/components/dialogs/locationInCupboardDialogs/CupboardLocationsDialog";

export default function CupboardsInRooms() {
	const { t: tCupboardInRoom } = useTranslation("cupboardinroom");
	const { t: tCupboard } = useTranslation("cupboard");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data: cupboardsInRooms, error } = useCupboardsInRooms();
	const { data: allCupboards } = useCupboards();
	const { data: rooms } = useRooms();

	const crud = useEntityCrud<
		ICupboardInRoomWithNames,
		ICupboardInRoomAdd,
		ICupboardInRoom
	>({
		service: cupboardsInRoomsService,
		invalidateKeys: [qk.cupboardsInRooms()],
	});

	const data: ICupboardInRoomWithNames[] = useMemo(() => {
		if (!cupboardsInRooms) return [];

		const cupboardMap = new Map(
			(allCupboards ?? []).map((c) => [c.id, c.codeName]),
		);
		const roomMap = new Map((rooms ?? []).map((r) => [r.id, r.roomName]));

		return cupboardsInRooms.map((r) => ({
			...r,
			roomName: roomMap.get(r.roomId) ?? r.roomId,
			codeName: cupboardMap.get(r.cupboardId) ?? r.cupboardId,
		}));
	}, [cupboardsInRooms, allCupboards, rooms]);

	// The create dialog only offers cupboards that aren't placed in a room yet.
	const availableCupboards = useMemo(() => {
		const used = new Set((cupboardsInRooms ?? []).map((d) => d.cupboardId));
		return (allCupboards ?? []).filter((c) => !used.has(c.id));
	}, [cupboardsInRooms, allCupboards]);

	const createOptions = useMemo(
		() => ({
			cupboards: cupboardsToOptions(availableCupboards),
			rooms: roomsToOptions(rooms ?? []),
		}),
		[availableCupboards, rooms],
	);

	// The edit dialog renders the cupboard as a readonly field, so only the room
	// list is selectable.
	const editOptions = useMemo(
		() => ({ rooms: roomsToOptions(rooms ?? []) }),
		[rooms],
	);

	// A cupboard can be created straight from this page. It lives outside the
	// entity CRUD trio: different service, and it invalidates only qk.cupboards().
	const queryClient = useQueryClient();
	const createCupboard = useMutation({
		mutationFn: (dto: ICupboardAdd) => unwrap(cupboardService.addAsync(dto)),
		onSuccess: () =>
			queryClient.invalidateQueries({ queryKey: qk.cupboards() }),
	});
	const [showCreateCupboard, setShowCreateCupboard] = useState(false);
	const handleCreateCupboard = makeConfirmHandler(
		createCupboard.mutateAsync,
		() => setShowCreateCupboard(false),
	);

	const [showLocations, setShowLocations] = useState(false);
	const [selectedCupboard, setSelectedCupboard] = useState<{
		id: string;
		codeName: string;
	} | null>(null);

	const columns = [
		tCupboardInRoom("Room"),
		tCupboardInRoom("Cupboard"),
		tCommon("Comment"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.roomName,
			item.codeName,
			item.comment || "-",
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => crud.openEdit(item)}
							/>
							<button
								type="button"
								onClick={() => {
									setSelectedCupboard({
										id: item.cupboardId,
										codeName: item.codeName,
									});
									setShowLocations(true);
								}}
								className="text-sm font-medium py-2 px-4 rounded-xl whitespace-nowrap transition-colors duration-150 text-center block w-full bg-[#eeeeee] hover:bg-gray-200 text-[#616161]"
							>
								{tCommon("DetailsLink")}
							</button>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => crud.openDelete(item)}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tCupboardInRoom("CupboardsInRooms")}
			createButton={
				canManage && (
					<div className="flex gap-2">
						<CreateButton
							onClick={() => setShowCreateCupboard(true)}
							label={tCupboard("NewCupboard")}
							variant="outline"
						/>
						<CreateButton onClick={crud.openCreate} />
					</div>
				)
			}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} minWidth="min-w-[600px]" />

			<EntityFormDialog
				open={showCreateCupboard}
				mode="create"
				config={cupboardFormConfig}
				onClose={() => setShowCreateCupboard(false)}
				onConfirm={handleCreateCupboard}
				isLoading={createCupboard.isPending}
			/>

			<EntityFormDialog
				mode="create"
				config={cupboardInRoomCreateConfig}
				options={createOptions}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={cupboardInRoomEditConfig}
				toForm={cupboardInRoomToForm}
				toUpdate={cupboardInRoomToUpdate}
				options={editOptions}
				staticValues={{ codeName: crud.entityToEdit?.codeName ?? "" }}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={cupboardInRoomCreateConfig.namespace}
				singularKey={cupboardInRoomCreateConfig.singularKey}
				summaryFields={cupboardInRoomDeleteSummary}
				{...crud.deleteDialogProps}
			/>

			<CupboardLocationsDialog
				open={showLocations}
				cupboard={selectedCupboard}
				onClose={() => {
					setShowLocations(false);
					setSelectedCupboard(null);
				}}
			/>
		</ListPageWrapper>
	);
}
