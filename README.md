1. в консоли перейти в папку проекта и ввести "docker-compose up --build"
2. Открыть Docker Desctop, там во containers найти контейнер libraryapi, запустить в нём backend и frontend, база данных запущена.
3. В браузере перейти по адресу:
        - Backend: `http://localhost:8080`
   - Frontend: `http://localhost:3000`
База данных создается при запуске приложения, в таблицу пользователей добавляется админ и user.
Логин/пароль для админа - admin1@gmail.com/Admin123!
Логин/пароль для юзера - user1@gmail.com/User123!
