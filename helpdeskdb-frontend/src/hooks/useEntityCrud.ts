"use client";

import { useState } from "react";
import { QueryKey, useMutation, useQueryClient } from "@tanstack/react-query";
import { ConfirmResult } from "@/components/dialogs/common/entityDialogTypes";
import { getErrorMessage, unwrap } from "@/services/errors";
import { IDomainId } from "@/types/IDomainId";
import { IResultObject } from "@/types/IResultObject";

/**
 * The slice of a service `useEntityCrud` needs. `EntityService` satisfies it
 * structurally; services with bespoke method sets can too.
 */
export interface EntityCrudService<TAdd, TUpdate> {
	addAsync(dto: TAdd): Promise<IResultObject<unknown>>;
	updateAsync(dto: TUpdate): Promise<IResultObject<unknown>>;
	deleteAsync(id: string): Promise<IResultObject<unknown>>;
}

/**
 * Wraps a mutation into the dialogs' ConfirmResult contract: resolve `void`
 * on success (after `onSuccess`, typically closing the dialog), resolve
 * `{ error }` on failure so the dialog renders it inline and stays open.
 * For page mutations that fall outside `useEntityCrud`'s create/edit/delete
 * trio (e.g. cupboardsInRooms' extra create-cupboard dialog).
 */
export function makeConfirmHandler<TVars>(
	mutateAsync: (vars: TVars) => Promise<unknown>,
	onSuccess?: () => void,
): (vars: TVars) => ConfirmResult {
	return async (vars) => {
		try {
			await mutateAsync(vars);
			onSuccess?.();
		} catch (error) {
			return { error: getErrorMessage(error) };
		}
	};
}

/**
 * Create/edit/delete plumbing shared by every entity list page: the three
 * mutations (invalidating `invalidateKeys` on success), the dialog selection
 * state, and the ConfirmResult handlers. List queries, enrichment joins, and
 * columns/rows stay in the pages — that's where pages genuinely differ.
 *
 * The `…DialogProps` bundles match the generic dialogs' prop names, so pages
 * spread them: `<EntityFormDialog mode="create" config={…} {...crud.createDialogProps} />`.
 *
 * `TUpdate` is the DTO `updateAsync` receives (defaults to `TEntity`;
 * EntityEditDialog builds it as `{ id, ...form }` or via its `toUpdate`
 * override).
 */
export function useEntityCrud<
	TEntity extends IDomainId,
	TAdd,
	TUpdate extends IDomainId = TEntity,
>(options: {
	service: EntityCrudService<TAdd, TUpdate>;
	invalidateKeys: readonly QueryKey[];
	/** Applied to every create DTO (e.g. the `…Assets` pages inject `createdBy`). */
	decorateCreate?: (dto: TAdd) => TAdd;
}) {
	const { service, invalidateKeys, decorateCreate } = options;
	const queryClient = useQueryClient();

	const invalidate = () => {
		for (const queryKey of invalidateKeys) {
			queryClient.invalidateQueries({ queryKey });
		}
	};

	const createMutation = useMutation({
		mutationFn: (dto: TAdd) =>
			unwrap(service.addAsync(decorateCreate ? decorateCreate(dto) : dto)),
		onSuccess: invalidate,
	});
	const editMutation = useMutation({
		mutationFn: (dto: TUpdate) => unwrap(service.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteMutation = useMutation({
		mutationFn: (id: string) => unwrap(service.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [entityToEdit, setEntityToEdit] = useState<TEntity | null>(null);
	const [entityToDelete, setEntityToDelete] = useState<TEntity | null>(null);

	// mutateAsync (not mutate) so a failure rejects and can be surfaced
	// through the dialogs' ConfirmResult contract.
	const handleCreate = async (dto: TAdd): ConfirmResult => {
		try {
			await createMutation.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: getErrorMessage(error) };
		}
	};

	const handleEdit = async (dto: TUpdate): ConfirmResult => {
		try {
			await editMutation.mutateAsync(dto);
			setEntityToEdit(null);
		} catch (error) {
			return { error: getErrorMessage(error) };
		}
	};

	const handleDelete = async (id: string): ConfirmResult => {
		try {
			await deleteMutation.mutateAsync(id);
			setEntityToDelete(null);
		} catch (error) {
			return { error: getErrorMessage(error) };
		}
	};

	return {
		invalidate,

		showCreate,
		openCreate: () => setShowCreate(true),
		handleCreate,
		createDialogProps: {
			open: showCreate,
			onClose: () => setShowCreate(false),
			onConfirm: handleCreate,
			isLoading: createMutation.isPending,
		},

		entityToEdit,
		openEdit: (entity: TEntity) => setEntityToEdit(entity),
		handleEdit,
		editDialogProps: {
			open: entityToEdit !== null,
			entity: entityToEdit,
			onClose: () => setEntityToEdit(null),
			onConfirm: handleEdit,
			isLoading: editMutation.isPending,
		},

		entityToDelete,
		openDelete: (entity: TEntity) => setEntityToDelete(entity),
		handleDelete,
		deleteDialogProps: {
			open: entityToDelete !== null,
			entity: entityToDelete,
			onClose: () => setEntityToDelete(null),
			onConfirm: handleDelete,
			isLoading: deleteMutation.isPending,
		},

		// Escape hatches for pages that need mutation state directly.
		createMutation,
		editMutation,
		deleteMutation,
	};
}
