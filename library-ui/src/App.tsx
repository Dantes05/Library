import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import BooksList from "./pages/BooksList";
import Login from "./pages/Login";
import AddBook from "./pages/AddBook";
import BookPage from "./pages/BookPage"; 
import EditBook from "./pages/EditBookPage";
import ProfilePage from "./pages/ProfilePage";

const App: React.FC = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/books" element={<BooksList />} />
        <Route path="/add-book" element={<AddBook />} />
        <Route path="/books/:id" element={<BookPage />} />
        <Route path="/edit-book/:id" element={<EditBook />} />
        <Route path="/profile" element={<ProfilePage />} />
      </Routes>
    </Router>
  );
};

export default App;
