import { IOwner } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { ownerDeleteSummary } from "../entityConfigs/owner";

interface DeleteOwnerDialogProps {
	open: boolean;
	owner: IOwner | null;
	onClose: () => void;
	onConfirm: (ownerId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteOwnerDialog = ({
	open,
	owner,
	onClose,
	onConfirm,
	isLoading,
}: DeleteOwnerDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={owner}
		namespace="owner"
		singularKey="OwnerSingular"
		summaryFields={ownerDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
