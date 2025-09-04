document.addEventListener('DOMContentLoaded', function () {
    // Инициализация всех селекторов на странице
    document.querySelectorAll('.universal-selector').forEach(initUniversalSelector);
});

function initUniversalSelector(container) {
    const searchInput = container.querySelector('.universal-search');
    const suggestionsList = container.querySelector('.universal-suggestions');
    const selectedContainer = container.querySelector('.selected-items');
    const addButton = container.querySelector('.universal-add-btn');
    const createModal = container.querySelector('.universal-create-modal');
    const newItemInput = container.querySelector('.universal-new-item');
    const confirmCreateBtn = container.querySelector('.universal-confirm-create');

    const fieldName = container.dataset.fieldName;
    const endpoint = searchInput.dataset.endpoint;
    const allowCreate = searchInput.dataset.allowCreate === 'true';

    let debounceTimeout;

    // ДЕЛЕГИРОВАНИЕ СОБЫТИЙ ДЛЯ УДАЛЕНИЯ - ИСПРАВЛЕНИЕ
    selectedContainer.addEventListener('click', function (e) {
        if (e.target.classList.contains('remove-btn')) {
            const chip = e.target.closest('.selected-item-chip');
            if (chip) {
                chip.remove();
            }
            e.stopPropagation();
        }
    });

    // Поиск с debounce
    searchInput.addEventListener('input', function () {
        clearTimeout(debounceTimeout);
        const query = this.value.trim();

        if (!query) {
            suggestionsList.style.display = 'none';
            return;
        }

        debounceTimeout = setTimeout(async () => {
            try {
                const response = await fetch(`${endpoint}?field=${fieldName}&query=${encodeURIComponent(query)}`);
                const items = await response.json();
                renderSuggestions(items, query);
            } catch (error) {
                console.error('Search error:', error);
            }
        }, 300);
    });

    // Рендер подсказок
    function renderSuggestions(items, query) {
        suggestionsList.innerHTML = '';

        if (items.length === 0 && allowCreate) {
            const li = document.createElement('li');
            li.className = 'list-group-item text-muted';
            /*li.innerHTML = `Нажмите Enter или кнопку "+" чтобы добавить "<strong>${query}</strong>"`;
            li.addEventListener('click', () => showCreateModal(query));*/
            li.innerHTML = `Не найдено`
            suggestionsList.appendChild(li);
        } else {
            items.forEach(item => {
                if (!isItemSelected(item.value)) {
                    const li = document.createElement('li');
                    li.className = 'list-group-item';
                    li.textContent = item.text;
                    li.dataset.value = item.value;
                    li.addEventListener('click', () => addSelectedItem(item));
                    suggestionsList.appendChild(li);
                }
            });
        }

        suggestionsList.style.display = 'block';
    }

    // Добавление по Enter
    searchInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const query = this.value.trim();
            if (query && allowCreate) {
                showCreateModal(query);
            }
        }
    });

    // Показать модальное окно создания
    function showCreateModal(prefilledValue = '') {
        if (createModal) {
            newItemInput.value = prefilledValue;
            new bootstrap.Modal(createModal).show();
        }
    }

    // Кнопка добавления
    if (addButton) {
        addButton.addEventListener('click', () => {
            const query = searchInput.value.trim();
            if (query) {
                showCreateModal(query);
            }
        });
    }

    // Подтверждение создания
    if (confirmCreateBtn) {
        confirmCreateBtn.addEventListener('click', async () => {
            const newValue = newItemInput.value.trim();
            if (newValue) {
                try {
                    // Отправляем на сервер для создания
                    const response = await fetch('/Universal/Create', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            field: fieldName,
                            value: newValue
                        })
                    });

                    if (response.ok) {
                        const newItem = await response.json();
                        addSelectedItem(newItem);
                        bootstrap.Modal.getInstance(createModal).hide();
                        newItemInput.value = '';
                        searchInput.value = '';
                    }
                } catch (error) {
                    console.error('Create error:', error);
                }
            }
        });
    }

    // Добавление выбранного элемента
    function addSelectedItem(item) {
        if (!isItemSelected(item.value)) {
            const chip = document.createElement('div');
            chip.className = 'selected-item-chip';
            chip.dataset.value = item.value;
            chip.innerHTML = `
                ${item.text}
                <span class="remove-btn">&times;</span>
                <input type="hidden" name="${fieldName}" value="${item.value}" />
            `;

            // Обработчик для новых элементов (для надежности)
            chip.querySelector('.remove-btn').addEventListener('click', () => {
                chip.remove();
            });

            selectedContainer.appendChild(chip);
            searchInput.value = '';
            suggestionsList.style.display = 'none';
        }
    }

    // Проверка, выбран ли уже элемент
    function isItemSelected(value) {
        return !!selectedContainer.querySelector(`[data-value="${value}"]`);
    }

    // Закрытие подсказок при клике вне
    document.addEventListener('click', function (e) {
        if (!container.contains(e.target)) {
            suggestionsList.style.display = 'none';
        }
    });
}