import { useMemo } from "react";
import {
	ILocation,
	ILocationInCupboardAdd,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	locationInCupboardFormConfig,
	locationsToOptions,
} from "../entityConfigs/locationInCupboard";

interface CreateLocationInCupboardDialogProps {
	open: boolean;
	cupboardId: string;
	cupboardCodeName: string;
	availableLocations: ILocation[];
	onClose: () => void;
	onConfirm: (
		data: ILocationInCupboardAdd,
	) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateLocationInCupboardDialog = ({
	open,
	cupboardId,
	cupboardCodeName,
	availableLocations,
	onClose,
	onConfirm,
	isLoading,
}: CreateLocationInCupboardDialogProps) => {
	const options = useMemo(
		() => ({ locations: locationsToOptions(availableLocations) }),
		[availableLocations],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="create"
			config={locationInCupboardFormConfig}
			options={options}
			staticValues={{ cupboardCodeName }}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({ locationId: data.locationId, cupboardId })
			}
			isLoading={isLoading}
		/>
	);
};
