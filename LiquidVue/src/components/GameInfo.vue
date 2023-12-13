<template>
  <div class="game-info">
    <img class="game-image" src="../assets/water-drop.png" alt="Game Image">
    <div class="info">
      <div class="game-name">{{ game.GameName }}</div>
      <div class="platform">{{ game.Platform }}</div>
    </div>
    <div class="bottom-info">
      <span class="release-year">{{ game.ReleaseYear }}</span>
      <span class="rank">{{ game.Rank }}</span>
      <span class="global-sales">{{ game.Global_Sales }}</span>
    </div>
  </div>
</template>


<script>
export default {
  props: {
    game: {
      type: Object,
      required: true
    }
  }
}
</script>

<style scoped>
.game-info {
  background-color: white;
  padding: 10px;
  margin: 10px;
  border-radius: 5px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 100%;
}

.game-info .game-image {
  width: 100%;
  height: 35%;
  object-fit: contain;
  object-position: center;
}

.info {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.info .game-name {
  font-size: 2em;
  text-align: center;
}

.info .platform {
  font-size: 1.5em;
  text-align: center;
}

.bottom-info {
  display: flex;
  justify-content: space-between;
  width: 100%;
}
</style>

