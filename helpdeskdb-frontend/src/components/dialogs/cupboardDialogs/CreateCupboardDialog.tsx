import { useState } from "react";
import { useTranslation } from "react-i18next";
import { SubmitHandler, useForm } from "react-hook-form";
import { Modal } from "../common/Modal";
import { ICupboardAdd } from "@/types/domain/DomainTypes";

interface CreateCupboardDialogProps {
    open: boolean;
    onClose: () => void;
    onConfirm: (data: ICupboardAdd) => Promise<{ error?: string } | void>;
    isLoading: boolean;
}

export const CreateCupboardDialog = ({
    open,
    onClose,
    onConfirm,
    isLoading,
}: CreateCupboardDialogProps) => {
    const { t: tCupboard } = useTranslation("cupboard");
    const { t: tCommon } = useTranslation("common");
    const { t: tValidation } = useTranslation("validationerrors");

    const [errorMsg, setErrorMsg] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<ICupboardAdd>({
        defaultValues: { codeName: ""},
    });

    const handleClose = () => {
        reset({ codeName: ""});
        setErrorMsg(null);
        onClose();
    };

    const onSubmit: SubmitHandler<ICupboardAdd> = async (data) => {
        setErrorMsg(null);
        const result = await onConfirm({
            codeName: data.codeName
        });
        if (result && result.error) {
            setErrorMsg(result.error);
            return;
        }
        reset({ codeName: "" });
    };

    return (
        <Modal open={open} onClose={handleClose}>
            <h2 className="text-xl font-bold mb-2 text-black">
                {tCommon("CreateTitle")}
            </h2>
            <h4 className="text-lg text-gray-700 mb-4">
                {tCupboard("CupboardSingular")}
            </h4>

            {errorMsg && (
                <div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
                    {errorMsg}
                </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div>
                    <label
                        htmlFor="codeName"
                        className="block text-gray-700 mb-2"
                    >
                        {tCupboard("CodeName")}
                    </label>
                    <input
                        id="codeName"
                        type="text"
                        className="w-full p-2 border border-gray-300 rounded"
                        placeholder={tCupboard("CodeNamePrompt")}
                        {...register("codeName", {
                            required: {
                                value: true,
                                message: tValidation("Required", {
                                    field: tCupboard("CodeName"),
                                }),
                            },
                            minLength: {
                                value: 2,
                                message: tValidation("MinLenghtValidationError", {
                                    field: tCupboard("CodeName"),
                                    min: 2,
                                }),
                            },
                            maxLength: {
                                value: 128,
                                message: tValidation("MaxLengthValidationError", {
                                    field: tCupboard("CodeName"),
                                    max: 128,
                                }),
                            },
                        })}
                    />
                    {errors.codeName && (
                        <span className="text-red-600 text-sm">
                            {errors.codeName.message}
                        </span>
                    )}
                </div>

                <div className="flex justify-end gap-3 mt-6">
                    <button
                        type="button"
                        onClick={handleClose}
                        disabled={isLoading}
                        className="px-4 py-2 bg-gray-400 hover:bg-gray-300 text-white rounded font-medium transition-colors"
                    >
                        {tCommon("Cancel")}
                    </button>
                    <button
                        type="submit"
                        disabled={isLoading}
                        className={`px-4 py-2 bg-orange-500 hover:bg-orange-600 font-medium text-white rounded transition-colors ${
                            isLoading ? "opacity-50 cursor-not-allowed" : ""
                        }`}
                    >
                        {isLoading ? tCommon("Processing") : tCommon("CreateButton")}
                    </button>
                </div>
            </form>
        </Modal>
    );
};
