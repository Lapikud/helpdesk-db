"use client";

import { useTranslation } from "react-i18next";
import { roomService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useRooms } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { IRoom, IRoomAdd } from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import ErrorBanner from "@/components/ErrorBanner";
import CreateButton from "@/components/CreateButton";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { EntityFormDialog } from "@/components/dialogs/common/EntityFormDialog";
import { EntityEditDialog } from "@/components/dialogs/common/EntityEditDialog";
import { EntityDeleteDialog } from "@/components/dialogs/common/EntityDeleteDialog";
import {
	roomDeleteSummary,
	roomFormConfig,
	roomToForm,
} from "@/components/dialogs/entityConfigs/room";

export default function Rooms() {
	const { t: tRoom } = useTranslation("room");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useRooms();
	const crud = useEntityCrud<IRoom, IRoomAdd>({
		service: roomService,
		invalidateKeys: [qk.rooms()],
	});

	const columns = [
		tRoom("RoomName"),
		tCommon("Comment"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.roomName,
			item.comment || "-",
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => crud.openEdit(item)}
							/>
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
			title={tRoom("Rooms")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={roomFormConfig}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={roomFormConfig}
				toForm={roomToForm}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={roomFormConfig.namespace}
				singularKey={roomFormConfig.singularKey}
				summaryFields={roomDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
