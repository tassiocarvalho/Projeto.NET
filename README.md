# Desafio Técnico - Aplicativo Dinâmico .NET MAUI

Este repositório contém a solução para o desafio técnico proposto, que consiste em um aplicativo .NET MAUI capaz de ler um arquivo JSON e montar dinamicamente sua interface de usuário com base nas especificações contidas nele.

## Visão Geral

O aplicativo implementa um construtor de formulários dinâmico. Ele lê o arquivo `layout.json` (localizado em `Resources/Raw/`) e gera abas (TabPages) e controles (TextBox, CheckBox, DropDown/Picker, etc.) conforme definido no JSON. Inclui também validação básica para campos marcados como obrigatórios (`ismandatory: true`).

**Observação Importante:** O desenvolvimento e compilação completos de aplicativos .NET MAUI (especialmente para iOS) requerem um ambiente macOS ou Windows com as devidas ferramentas instaladas. Este projeto foi desenvolvido em um ambiente Linux, que possui limitações. A lógica principal foi implementada, mas a compilação e execução devem ser feitas em um sistema operacional suportado.

## Requisitos Técnicos Atendidos

1.  **Importação e Interpretação JSON:** O aplicativo carrega e interpreta o `layout.json` embutido nos recursos da aplicação.
2.  **Montagem Dinâmica da Tela:** A `MainPage` (agora uma `TabbedPage`) cria dinamicamente as abas e os controles (Label, Entry para TextBox/Multiline, CheckBox, Picker para DropDown, DatePicker, Button para Photo/GPS placeholders, layouts para containers) com base no JSON.
3.  **Campos JSON Considerados:**
    *   `type`: Utilizado para determinar o tipo de controle a ser criado.
    *   `text`: Usado como título/label do controle.
    *   `ismandatory` (ou `obrigatorio` no PDF): Identifica campos obrigatórios e adiciona um indicador visual (*) e lógica de validação.
    *   `opcoes`: Populado no Picker (Dropdown) se presente (atualmente usa dados fictícios se ausente no JSON fornecido).
    *   `valorPadrao` / `initialvalue`: Usado para pré-preencher valores em controles como Entry e CheckBox.
4.  **Validação Amigável:** Um botão "Validate Page" foi adicionado a cada aba. Ao ser clicado, verifica se os campos marcados como obrigatórios naquela aba foram preenchidos e exibe uma mensagem de sucesso ou uma lista dos campos pendentes.

## Controles Suportados (e Mapeamento JSON -> MAUI)

*   `tabpage`: Mapeado para `ContentPage` dentro da `TabbedPage` principal.
*   `textbox`: Mapeado para `Entry`.
*   `multilinetextbox`: Mapeado para `Entry` (MAUI não tem `Entry` multilinhas nativo, `Editor` seria alternativa).
*   `checkbox`: Mapeado para `CheckBox`.
*   `dropdown`: Mapeado para `Picker`.
*   `datepicker`: Mapeado para `DatePicker` (adicionado conforme PDF, não presente no JSON de exemplo).
*   `label`: Mapeado para `Label` (adicionado conforme PDF).
*   `photo`, `gps`: Mapeados para `Button` como placeholders.
*   `itemscontainermaster`: Mapeado para `VerticalStackLayout` para agrupar sub-itens.
*   `radiobuttonlist`: Mapeado para `VerticalStackLayout` com `RadioButton`s (placeholder, pois não há controle direto).

## Como Executar o Projeto

**Pré-requisitos:**

*   **Ambiente:** Windows ou macOS.
*   **.NET SDK:** Versão 8.0 ou superior.
*   **Workload .NET MAUI:** Instalada (`dotnet workload install maui`).
*   **IDE (Recomendado):**
    *   Visual Studio 2022 (Windows)
    *   Visual Studio 2022 for Mac (macOS)
    *   Visual Studio Code com a extensão .NET MAUI.
*   **Para Android:** Android SDK configurado.
*   **Para iOS/MacCatalyst:** Xcode (macOS).

**Passos:**

1.  **Clone o Repositório (ou extraia o ZIP):**
    ```bash
    # Se for um repositório Git
    git clone <url_do_repositorio>
    cd DynamicMauiApp
    ```
    Se for um ZIP, extraia e navegue até a pasta `DynamicMauiApp`.

2.  **Restaure as Dependências:**
    ```bash
    dotnet restore
    ```

3.  **Compile e Execute:**

    *   **Via Linha de Comando (Exemplo para Android):**
        ```bash
        # Listar dispositivos/emuladores Android
        dotnet build -t:Run -f net8.0-android
        # Escolha um dispositivo/emulador da lista ou conecte um.
        # Execute no dispositivo/emulador escolhido (substitua <DEVICE_ID> se necessário)
        # dotnet build -t:Run -f net8.0-android -p:AndroidDeviceSerial=<DEVICE_ID>
        ```
    *   **Via Linha de Comando (Exemplo para iOS):** (Requer macOS)
        ```bash
        # Listar simuladores iOS
        dotnet build -t:Run -f net8.0-ios
        # Escolha um simulador
        # dotnet build -t:Run -f net8.0-ios -p:RuntimeIdentifier=ios-simulator
        ```
    *   **Via Visual Studio / VS for Mac:**
        *   Abra o arquivo `DynamicMauiApp.sln`.
        *   Selecione a plataforma de destino (Android, iOS, MacCatalyst, Windows) na barra de ferramentas.
        *   Selecione o dispositivo/emulador/simulador de destino.
        *   Pressione o botão de execução (Play/Debug).

## Estrutura do Projeto

*   `DynamicMauiApp.csproj`: Arquivo de projeto .NET MAUI.
*   `MauiProgram.cs`: Ponto de entrada da aplicação, configuração de serviços.
*   `App.xaml`/`App.xaml.cs`: Definição global da aplicação.
*   `AppShell.xaml`/`AppShell.xaml.cs`: Shell principal da aplicação (não utilizado diretamente para a lógica dinâmica aqui).
*   `MainPage.xaml`/`MainPage.xaml.cs`: A página principal (`TabbedPage`) onde a interface dinâmica é construída e a lógica de validação reside.
*   `Resources/Raw/layout.json`: O arquivo JSON que define a estrutura da interface.
*   `Platforms/`: Código específico de cada plataforma (Android, iOS, etc.).

## Critérios de Avaliação (Autoavaliação)

*   **Organização e Clareza:** O código em `MainPage.xaml.cs` está estruturado para ler o JSON e criar controles dinamicamente. Funções auxiliares foram usadas para melhorar a legibilidade.
*   **Aderência aos Requisitos:** A maioria dos requisitos técnicos do PDF foi atendida, incluindo leitura de JSON, criação dinâmica de controles comuns e validação básica.
*   **Usabilidade:** A interface é gerada em abas para melhor organização. A validação por página ajuda o usuário.
*   **Tratamento de Erros:** Tratamento básico de exceções na leitura do JSON e validação.
*   **Criatividade:** A solução implementa um gerador de formulário básico a partir do JSON.
*   **Escalabilidade:** A abordagem de criação dinâmica permite adicionar novos tipos de controle ou abas modificando o JSON e estendendo a lógica em `CreateControlFromJson`. A validação pode ser aprimorada.

## Próximos Passos / Melhorias Possíveis

*   Implementar suporte real para controles mais complexos (`multilinetextbox` usando `Editor`, `radiobuttonlist`, `photo`, `gps`).
*   Aprimorar a validação (tipos de dados, formatos, regras mais complexas).
*   Carregar opções de `dropdown` de fontes externas ou do próprio JSON.
*   Implementar a funcionalidade de clonagem para `itemscontainermaster`.
*   Melhorar o design e a experiência do usuário (estilos, layout responsivo).
*   Adicionar testes unitários e de interface.

