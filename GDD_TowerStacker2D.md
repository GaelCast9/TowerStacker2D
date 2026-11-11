# Tower Stacker 2D – Documento de Diseño (GDD)

## 1. Resumen
| Campo | Valor |
|---|---|
| Nombre | Tower Stacker 2D |
| Motor | Unity 6 (6000.6) – plantilla 2D (URP) |
| Género | Arcade / Física / Casual |
| Plataforma | PC (teclado y mouse), compatible con touch |
| Tiempo estimado | 2 – 4 horas |
| Arte | Figuras geométricas simples (cuadrados de colores) |

**Idea:** Una grúa se mueve de izquierda a derecha en la parte superior de la pantalla. El jugador suelta cajas para apilarlas sobre una base. Si una caja se cae de la torre, se acaba el juego. Gana el que logre la torre más alta.

## 2. Controles
| Acción | Tecla |
|---|---|
| Soltar caja | `Espacio`, clic izquierdo o tocar la pantalla |
| Reiniciar (en Game Over) | `Espacio`, clic izquierdo o tocar la pantalla |

## 3. Reglas del juego
1. La grúa se mueve sola de lado a lado (movimiento ping-pong).
2. Al presionar el botón, la caja se suelta y cae con física real (`Rigidbody2D`).
3. Cada caja que aterriza suma **+1 punto**.
4. Cada punto hace que la grúa se mueva **un poco más rápido** (dificultad progresiva).
5. Las cajas tienen **ancho aleatorio** y **color aleatorio** para dar variedad.
6. Si **cualquier caja cae por debajo de la base** → **Game Over**.
7. Se guarda el **récord (Best)** con `PlayerPrefs`.
8. La cámara **sube suavemente** conforme crece la torre.

## 4. Elementos de la escena
| Objeto | Componentes | Función |
|---|---|---|
| `Main Camera` | Camera, `CameraFollow` | Sigue la altura de la torre |
| `GameManager` | `GameManager` | Puntaje, récord, Game Over, reinicio, UI |
| `Crane` (Grúa) | SpriteRenderer, `Crane` | Se mueve y suelta cajas |
| `Ground` (Base) | SpriteRenderer, BoxCollider2D | Pedestal donde se apila la torre |
| `Block` (Prefab) | SpriteRenderer, Rigidbody2D, BoxCollider2D, `Block` | La caja que se apila |

## 5. Scripts (`Assets/Scripts/`)
| Script | Responsabilidad |
|---|---|
| `GameManager.cs` | Singleton. Puntaje, récord, altura máxima, Game Over, reinicio, UI (OnGUI) y lectura de input |
| `Crane.cs` | Movimiento ping-pong, generar caja, soltar caja, aumentar velocidad |
| `Block.cs` | Detecta cuando aterriza (suma punto) y cuando se cae (Game Over) |
| `CameraFollow.cs` | Sube la cámara suavemente según la altura de la torre |

## 6. Assets
| Asset | Ruta |
|---|---|
| Sprite cuadrado blanco | `Assets/Sprites/Square.png` |
| Material físico (fricción alta, sin rebote) | `Assets/Physics/BlockMaterial.physicsMaterial2D` |
| Prefab de caja | `Assets/Prefabs/Block.prefab` |
| Escena principal | `Assets/Scenes/SampleScene.unity` |

## 7. Checklist de desarrollo
### Fase 1 – Núcleo (MVP)
- [x] Crear proyecto `TowerStacker2D`
- [x] Conectar MCP for Unity
- [x] Sprite cuadrado y material físico
- [x] Base (`Ground`)
- [x] Prefab `Block` con física
- [x] Grúa que se mueve y suelta cajas
- [x] Detección de aterrizaje y puntaje
- [x] Game Over al caer una caja
- [x] Reinicio

### Fase 2 – Pulido rápido
- [x] Cámara que sube con la torre
- [x] Velocidad progresiva
- [x] Colores y anchos aleatorios
- [x] Récord con `PlayerPrefs`
- [x] Color de fondo agradable

### Fase 3 – Diseño y "juice"
- [x] Sonidos procedurales (soltar, aterrizar, perfecto, inicio, Game Over) – `AudioManager.cs`
- [x] Partículas: polvo al aterrizar, chispas en Perfecto, confeti por récord – `FXManager.cs`
- [x] Bonus **PERFECTO** (caja centrada ±0.12) con **combo** acumulable (+1 + combo)
- [x] Anillo dorado que se expande en Perfecto
- [x] Menú de inicio (título, instrucciones, récord)
- [x] Pantalla de Game Over con zoom-out para ver la torre completa
- [x] Cajas con bordes redondeados (sprite 9-slice) y animación de aparición
- [x] Destello blanco al aterrizar + camera shake
- [x] Textos flotantes (+1, ¡PERFECTO! x2)
- [x] Cielo dinámico: día → atardecer → noche → espacio – `BackgroundFX.cs`
- [x] Nubes con parallax, sol que se oculta, estrellas que parpadean
- [x] Paisaje: colinas, ciudad y pasto
- [x] Grúa con riel, ruedas, gancho y línea guía de caída
- [ ] Build para PC / WebGL

### Fase 4 – Entrega
- [ ] Subir proyecto a Git (Unity Hub → *Connect to source control*)
- [ ] `.gitignore` de Unity (excluir `Library/`, `Temp/`, `Logs/`, `UserSettings/`)
