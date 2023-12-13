<template>
  <div class="header">
    <div class="logo-container">
      <img class="water-drop" src="@/assets/water-drop.png" alt="waterdrop">
      <div class="title-container">
        <div class="text">
          <div class="liquid">LIQUID</div>
          <div class="games">Games</div>
        </div>
      </div>
    </div>
    <div class="line"/>
  </div>
</template>

<script>
</script>

<style scoped>
.header {
  width: 100vw;
  background-color: #e8e8e8;
  margin-bottom: 20px;
}

.logo-container {
  display: flex;
  align-items: center;
}

.title-container {
  width: 100vw;
  color: black;
}

.text {
  display: flex;
}

.liquid {
  font-size: 9vh;
  margin-right: 5px;
}

.games {
  padding-top: 4.5vh;
  font-size: 4vh;
}

.water-drop {
  height: 90px;
  width: auto;
  padding: 20px;
}

.line{
  width: 100vw;
  background-color: #9c9b9b;
  height: 2px;
}
</style>