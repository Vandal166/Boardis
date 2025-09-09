import React, { createContext, useContext, useState, type ReactNode } from 'react';

interface ConfirmationOptions
{
    title?: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
}

interface ConfirmationDialogContextValue
{
    confirm: ConfirmFn;
    open: boolean;
}

type ConfirmFn = (options: ConfirmationOptions) => Promise<boolean>;

const ConfirmationDialogContext = createContext<ConfirmationDialogContextValue | undefined>(undefined);

export const useConfirmation = () =>
{
    const ctx = useContext(ConfirmationDialogContext);
    if (!ctx)
        throw new Error('useConfirmation must be used within ConfirmationDialogProvider');
    return ctx;
};

export const useConfirmationDialogOpen = () =>
{
    const ctx = useContext(ConfirmationDialogContext);
    if (!ctx)
        throw new Error('useConfirmationDialogOpen must be used within ConfirmationDialogProvider');
    return ctx.open;
};

export const ConfirmationDialogProvider: React.FC<{ children: ReactNode }> = ({ children }) =>
{
    const [open, setOpen] = useState(false);
    const [options, setOptions] = useState<ConfirmationOptions | null>(null);
    const [resolvePromise, setResolvePromise] = useState<((result: boolean) => void) | null>(null);

    const confirm: ConfirmFn = (opts) =>
    {
        setOptions(opts);
        setOpen(true);
        return new Promise<boolean>((resolve) => setResolvePromise(() => resolve));
    };

    const handleClose = (result: boolean) =>
    {
        setOpen(false);
        setTimeout(() =>
        {
            setOptions(null);
            resolvePromise?.(result);
        }, 200);
    };

    return (
        <ConfirmationDialogContext.Provider value={{ confirm, open }}>
            {children}
            {open && options && (
                <div className="fixed inset-0 z-1000 flex items-center justify-center">
                    <div className="absolute inset-0 bg-black opacity-40" />
                    <div className="relative bg-white text-black rounded-lg shadow-xl p-6 z-1000 min-w-[320px]">
                        <h3 className="font-bold text-lg mb-2">{options.title || 'Are you sure?'}</h3>
                        <div className="mb-4">{options.message}</div>
                        <div className="flex justify-end gap-2">
                            <button
                                className="px-4 py-2 rounded bg-red-500 hover:bg-red-600"
                                onClick={() => handleClose(true)}
                            >
                                {options.confirmText || 'Confirm'}
                            </button>
                            <button
                                className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300"
                                onClick={() => handleClose(false)}
                            >
                                {options.cancelText || 'Cancel'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </ConfirmationDialogContext.Provider>
    );
};