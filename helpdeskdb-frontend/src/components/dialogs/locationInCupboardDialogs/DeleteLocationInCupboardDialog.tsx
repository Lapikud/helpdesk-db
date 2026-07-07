import { ILocationInCupboardWithNames } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { locationInCupboardDeleteSummary } from "../entityConfigs/locationInCupboard";

interface DeleteLocationInCupboardDialogProps {
	open: boolean;
	entry: ILocationInCupboardWithNames | null;
	onClose: () => void;
	onConfirm: (id: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteLocationInCupboardDialog = ({
	open,
	entry,
	onClose,
	onConfirm,
	isLoading,
}: DeleteLocationInCupboardDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={entry}
		namespace="locationincupboard"
		singularKey="LocationInCupboardSingular"
		summaryFields={locationInCupboardDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
