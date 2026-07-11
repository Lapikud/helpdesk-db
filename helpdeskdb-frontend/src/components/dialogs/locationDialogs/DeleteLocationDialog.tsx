import { ILocation } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { locationDeleteSummary } from "../entityConfigs/location";

interface DeleteLocationDialogProps {
	open: boolean;
	location: ILocation | null;
	onClose: () => void;
	onConfirm: (locationId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteLocationDialog = ({
	open,
	location,
	onClose,
	onConfirm,
	isLoading,
}: DeleteLocationDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={location}
		namespace="location"
		singularKey="LocationSingular"
		summaryFields={locationDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
