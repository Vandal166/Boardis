import React, { useState } from 'react';


interface AddBoardMemberButtonProps
{
    onAdd: (emailOrUsername: string) => void;
    isLoading?: boolean;
}

const AddBoardMemberButton: React.FC<AddBoardMemberButtonProps> = ({
    onAdd,
    isLoading = false,
}) =>
{
    const [input, setInput] = useState('');


    const handleAdd = (e?: React.FormEvent) =>
    {
        e?.preventDefault();
        onAdd(input.trim());
        setInput('');
    };

    return (
        <form className="flex gap-2 mb-4" onSubmit={handleAdd}>
            <input
                type="text"
                className="flex-1 border rounded px-3 py-2"
                placeholder="Email or username"
                required
                value={input}
                onChange={e => setInput(e.target.value)}
                disabled={isLoading}
            />
            <button
                type="submit"
                className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
                disabled={isLoading}
            >
                Add
            </button>
        </form>
    );
};

export default AddBoardMemberButton;