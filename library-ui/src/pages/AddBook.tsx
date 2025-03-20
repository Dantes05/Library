import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

const AddBook: React.FC = () => {
  const [isbn, setIsbn] = useState("");
  const [title, setTitle] = useState("");
  const [genre, setGenre] = useState("");
  const [description, setDescription] = useState("");
  const [authorId, setAuthorId] = useState<string | null>(null);
  const [authors, setAuthors] = useState<{ id: number; firstName: string; lastName: string }[]>([]);
  const [showAddAuthor, setShowAddAuthor] = useState(false);
  const [newAuthor, setNewAuthor] = useState({
    firstName: "",
    lastName: "",
    birthDate: "",
    country: "",
  });
  const [image, setImage] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [authorError, setAuthorError] = useState<string | null>(null);

  const navigate = useNavigate();

  useEffect(() => {
    fetchAuthors();
  }, []);

  const fetchAuthors = async () => {
    try {
      const response = await axios.get("http://localhost:7143/api/authors");
      setAuthors(response.data);
    } catch (error) {
      console.error("Ошибка при загрузке авторов:", error);
    }
  };

  const handleAddAuthor = async () => {
    setAuthorError(null);

    if (!newAuthor.firstName || !newAuthor.lastName || !newAuthor.birthDate || !newAuthor.country) {
      setAuthorError("Все поля автора должны быть заполнены.");
      return;
    }

    try {
      const response = await axios.post("http://localhost:7143/api/authors", newAuthor);
      const addedAuthor = response.data;

      setAuthors([...authors, addedAuthor]);
      setAuthorId(addedAuthor.id.toString());
      setShowAddAuthor(false);

      console.log("✅ Автор добавлен:", addedAuthor);
    } catch (error) {
      console.error("Ошибка при добавлении автора:", error);
    }
  };

  const handleAddBook = async () => {
    setError(null);

    if (!isbn || !title || !genre || !description || !authorId) {
      setError("Все поля должны быть заполнены.");
      return;
    }

    try {
      const token = localStorage.getItem("token");
      if (!token) {
        alert("Пользователь не авторизован.");
        navigate("/login");
        return;
      }

      const formData = new FormData();
      formData.append("ISBN", isbn);
      formData.append("Title", title);
      formData.append("Genre", genre);
      formData.append("Description", description);
      formData.append("AuthorId", authorId);
      if (image) {
        formData.append("Image", image);
      }

      const response = await axios.post("http://localhost:7143/api/books", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
          Authorization: `Bearer ${token}`,
        },
      });

      console.log("Книга успешно добавлена:", response.data);
      navigate("/books");
    } catch (error) {
      console.error("Ошибка при добавлении книги:", error);
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Добавление книги</h1>

      {error && <p className="text-red-500 mb-4">{error}</p>}

      <input className="border p-2 mb-2 w-full" type="text" placeholder="ISBN" value={isbn} onChange={(e) => setIsbn(e.target.value)} />
      <input className="border p-2 mb-2 w-full" type="text" placeholder="Название книги" value={title} onChange={(e) => setTitle(e.target.value)} />
      <input className="border p-2 mb-2 w-full" type="text" placeholder="Жанр" value={genre} onChange={(e) => setGenre(e.target.value)} />
      <textarea className="border p-2 mb-2 w-full" placeholder="Описание" value={description} onChange={(e) => setDescription(e.target.value)} />

      <input
        className="border p-2 mb-2 w-full"
        type="file"
        accept="image/*"
        onChange={(e) => {
          if (e.target.files && e.target.files[0]) {
            setImage(e.target.files[0]);
          }
        }}
      />

      <select className="border p-2 mb-2 w-full" value={authorId || ""} onChange={(e) => setAuthorId(e.target.value)}>
        <option value="">Выберите автора</option>
        {authors.map((author) => (
          <option key={author.id} value={author.id}>
            {author.firstName} {author.lastName}
          </option>
        ))}
      </select>

      <button className="bg-gray-500 text-white px-4 py-2 mb-2" onClick={() => setShowAddAuthor(true)}>
        ➕ Добавить нового автора
      </button>

      {showAddAuthor && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded-lg shadow-lg">
            <h2 className="text-xl font-bold mb-4">Добавление автора</h2>
            {authorError && <p className="text-red-500 mb-4">{authorError}</p>}
            <input className="border p-2 mb-2 w-full" type="text" placeholder="Имя" value={newAuthor.firstName} onChange={(e) => setNewAuthor({ ...newAuthor, firstName: e.target.value })} />
            <input className="border p-2 mb-2 w-full" type="text" placeholder="Фамилия" value={newAuthor.lastName} onChange={(e) => setNewAuthor({ ...newAuthor, lastName: e.target.value })} />
            <input className="border p-2 mb-2 w-full" type="date" placeholder="Дата рождения" value={newAuthor.birthDate} onChange={(e) => setNewAuthor({ ...newAuthor, birthDate: e.target.value })} />
            <input className="border p-2 mb-2 w-full" type="text" placeholder="Страна" value={newAuthor.country} onChange={(e) => setNewAuthor({ ...newAuthor, country: e.target.value })} />

            <div className="flex justify-between mt-4">
              <button className="bg-green-500 text-white px-4 py-2" onClick={handleAddAuthor}>
                ✅ Добавить автора
              </button>
              <button className="bg-red-500 text-white px-4 py-2" onClick={() => setShowAddAuthor(false)}>
                ❌ Закрыть
              </button>
            </div>
          </div>
        </div>
      )}

      <button className="bg-green-500 text-white px-4 py-2 mt-4" onClick={handleAddBook}>
        📚 Добавить книгу
      </button>
    </div>
  );
};

export default AddBook;