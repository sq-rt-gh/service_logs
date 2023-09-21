## Предыстория

В компании есть несколько сервисов. Для удобства поддержки эти сервисы пишут логи в виде текстовых файлов. Чтобы быстро работать с файлами логов они ротируются, то есть, когда размер файла лога превышает некоторый порог в S байт – он переименовывается с добавлением номера ротации в имя, а текущий лог направляется в новый файл. Для экономии места система хранит не больше N ротации для каждого сервиса.

### Формат логов

Имя текущего файла логов для сервиса: <service_name>.log.

Где service_name – имя сервиса.

Имя предыдущих файлов логов для сервиса: <service_name>.<rotation_number>.log.

Где rotation_number равный 1 – предыдущий файл логов, а равный N – самый старый файл. Например: AwesomeService.13.log.

### Формат записи лога: 

[Дата Время][Категория_записи] Текст_записи.

Пример:

[23.03.2023 14:00:00.235][RequestHandler] New request with 42 items received from user vlad@redsquare.ru.

## Задача

Компания поставила вам задачу реализовать сервис, генерирующий отчёты по логам сервисов.
На вход сервису должен подаваться запрос на поиск по имени сервиса с поддержкой регулярных выражений. Запрос также содержит путь к директории с файлами логов.
В ответ сервис возвращает коллекцию отчётов, для всех сервисов, попадающих под условия поиска.

### Структура отчёта:

  Имя сервиса

  Дата и время самой ранней записи в логах

  Дата и время самой последней записи в логах

  Количество записей в каждой категории

  Количество ротаций.

Форма реализации сервиса: REST API сервис или консольное приложение.

### Требования со звёздочкой (опциональные/бонусные)

  1. Асинхронная реализация, где запрос не блокирует клиента в ожидании, а получает идентификатор задачи, и предоставляется возможность периодически узнать статус готовности отчёта.

  2. Возможность посмотреть статус системы – количество запущенных задач и их идентификаторы (при выполнении требования 1)

  3. Загрузка логов в систему с анонимизацией персональных данных (адресов электронной почты: example@domain.com -> ex*e@domain.com).
